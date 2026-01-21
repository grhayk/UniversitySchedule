using Application.Core;
using Application.Interfaces;
using Application.Models;
using CsvHelper;
using Domain.Entities;
using Domain.Enums;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Application.Features.Schedules.BulkUpload
{
    public class BulkUploadScheduleHandler : IRequestHandler<BulkUploadScheduleCommand, Result<BulkUploadResult>>
    {
        private readonly IDbContext _context;

        public BulkUploadScheduleHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result<BulkUploadResult>> Handle(BulkUploadScheduleCommand request, CancellationToken ct)
        {
            var result = new BulkUploadResult();

            try
            {
                using var reader = new StringReader(request.CsvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<CsvScheduleMap>();
                var csvRecords = csv.GetRecords<CsvScheduleRecord>().ToList();

                // Pre-load reference data
                var subjectIds = csvRecords.Select(r => r.SubjectId).Distinct().ToList();
                var lecturerIds = csvRecords.Select(r => r.LecturerId).Distinct().ToList();
                var classroomIds = csvRecords.Select(r => r.ClassroomId).Distinct().ToList();
                var timeTableIds = csvRecords.Select(r => r.TimeTableId).Distinct().ToList();
                var semesterIds = csvRecords.Select(r => r.SemesterId).Distinct().ToList();

                // Parse all groupIds from all records
                var allGroupIds = csvRecords
                    .SelectMany(r => ParseGroupIds(r.GroupIds))
                    .Distinct()
                    .ToList();

                // Load LecturerSubjects
                var lecturerSubjects = await _context.LecturerSubjects
                    .Where(ls => lecturerIds.Contains(ls.LecturerId) && subjectIds.Contains(ls.SubjectId))
                    .ToDictionaryAsync(ls => (ls.LecturerId, ls.SubjectId), ct);

                // Load TimeTables
                var validTimeTableIds = await _context.TimeTables
                    .Where(t => timeTableIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync(ct);

                // Load SubjectClassrooms
                var subjectClassrooms = await _context.SubjectClassrooms
                    .Where(sc => subjectIds.Contains(sc.SubjectId) && classroomIds.Contains(sc.ClassroomId))
                    .ToListAsync(ct);

                // Load Groups with SemesterIds
                var groups = await _context.Groups
                    .Where(g => allGroupIds.Contains(g.Id))
                    .ToDictionaryAsync(g => g.Id, ct);

                // Load valid Semesters
                var validSemesterIds = await _context.Semesters
                    .Where(s => semesterIds.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync(ct);

                // Load GroupSubjectsWithLecturer
                var groupSubjectsWithLecturer = await _context.GroupSubjectsWithLecturer
                    .Where(gsl => allGroupIds.Contains(gsl.GroupId))
                    .Include(gsl => gsl.LecturerSubject)
                    .ToListAsync(ct);

                // Load existing schedules for conflict checking
                var existingSchedules = await _context.Schedules
                    .Include(s => s.ScheduleGroups)
                    .ToListAsync(ct);

                // Track schedules added in this batch for conflict detection
                var batchSchedules = new List<(DateTime Date, int TimeTableId, int ClassroomId, int LecturerId, WeekType WeekType, List<int> GroupIds)>();

                var schedulesToAdd = new List<Schedule>();

                foreach (var (record, rowNumber) in csvRecords.Select((r, i) => (r, i + 2)))
                {
                    var validationResult = ValidateRecord(record);
                    if (!validationResult.IsValid)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                            Code = $"Row:{rowNumber}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Parse GroupIds
                    var groupIds = ParseGroupIds(record.GroupIds);
                    if (!groupIds.Any())
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = "Invalid GroupIds format. Use pipe-separated values (e.g., '1|2|3')",
                            Code = $"Row:{rowNumber}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate LecturerSubject
                    if (!lecturerSubjects.TryGetValue((record.LecturerId, record.SubjectId), out var lecturerSubject))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Lecturer {record.LecturerId} is not assigned to teach Subject {record.SubjectId}",
                            Code = $"LS:{record.LecturerId}-{record.SubjectId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate TimeTable
                    if (!validTimeTableIds.Contains(record.TimeTableId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"TimeTable with ID {record.TimeTableId} not found",
                            Code = $"TT:{record.TimeTableId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate Semester
                    if (!validSemesterIds.Contains(record.SemesterId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Semester with ID {record.SemesterId} not found",
                            Code = $"Sem:{record.SemesterId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate SubjectClassroom
                    var subjectClassroomExists = subjectClassrooms.Any(sc =>
                        sc.SubjectId == record.SubjectId &&
                        sc.LessonType == record.LessonType &&
                        sc.ClassroomId == record.ClassroomId);

                    if (!subjectClassroomExists)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Classroom {record.ClassroomId} is not configured for Subject {record.SubjectId} with LessonType {record.LessonType}",
                            Code = $"SC:{record.SubjectId}-{record.ClassroomId}-{record.LessonType}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate all groups exist and have correct semester
                    var missingGroups = groupIds.Where(gId => !groups.ContainsKey(gId)).ToList();
                    if (missingGroups.Any())
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Groups not found: {string.Join(", ", missingGroups)}",
                            Code = $"G:{string.Join("-", missingGroups)}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var invalidSemesterGroups = groupIds.Where(gId => groups[gId].SemesterId != record.SemesterId).ToList();
                    if (invalidSemesterGroups.Any())
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Groups {string.Join(", ", invalidSemesterGroups)} do not belong to Semester {record.SemesterId}",
                            Code = $"GS:{string.Join("-", invalidSemesterGroups)}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate GroupSubjectWithLecturer for each group
                    var groupsWithoutAssignment = groupIds.Where(gId =>
                        !groupSubjectsWithLecturer.Any(gsl =>
                            gsl.GroupId == gId &&
                            gsl.LecturerSubjectId == lecturerSubject.Id &&
                            gsl.LessonType == record.LessonType)).ToList();

                    if (groupsWithoutAssignment.Any())
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Groups {string.Join(", ", groupsWithoutAssignment)} are not assigned to Lecturer {record.LecturerId} for Subject {record.SubjectId} with LessonType {record.LessonType}",
                            Code = $"GSL:{string.Join("-", groupsWithoutAssignment)}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check conflicts with existing schedules
                    var classroomConflict = existingSchedules.Any(s =>
                        s.ScheduleDate.Date == record.ScheduleDate.Date &&
                        s.TimeTableId == record.TimeTableId &&
                        s.ClassroomId == record.ClassroomId &&
                        s.WeekType == record.WeekType);

                    if (classroomConflict)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Classroom {record.ClassroomId} is already scheduled at the same date, time slot, and week type",
                            Code = $"CC:{record.ClassroomId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var lecturerConflict = existingSchedules.Any(s =>
                        s.ScheduleDate.Date == record.ScheduleDate.Date &&
                        s.TimeTableId == record.TimeTableId &&
                        s.LecturerId == record.LecturerId &&
                        s.WeekType == record.WeekType);

                    if (lecturerConflict)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Lecturer {record.LecturerId} is already scheduled at the same date, time slot, and week type",
                            Code = $"LC:{record.LecturerId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var groupConflicts = existingSchedules
                        .Where(s => s.ScheduleDate.Date == record.ScheduleDate.Date &&
                                    s.TimeTableId == record.TimeTableId &&
                                    s.WeekType == record.WeekType)
                        .SelectMany(s => s.ScheduleGroups.Select(sg => sg.GroupId))
                        .Intersect(groupIds)
                        .ToList();

                    if (groupConflicts.Any())
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Groups {string.Join(", ", groupConflicts)} are already scheduled at the same date, time slot, and week type",
                            Code = $"GC:{string.Join("-", groupConflicts)}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check conflicts within batch
                    var batchClassroomConflict = batchSchedules.Any(b =>
                        b.Date.Date == record.ScheduleDate.Date &&
                        b.TimeTableId == record.TimeTableId &&
                        b.ClassroomId == record.ClassroomId &&
                        b.WeekType == record.WeekType);

                    if (batchClassroomConflict)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Classroom {record.ClassroomId} conflicts with another row in this CSV",
                            Code = $"BCC:{record.ClassroomId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var batchLecturerConflict = batchSchedules.Any(b =>
                        b.Date.Date == record.ScheduleDate.Date &&
                        b.TimeTableId == record.TimeTableId &&
                        b.LecturerId == record.LecturerId &&
                        b.WeekType == record.WeekType);

                    if (batchLecturerConflict)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Lecturer {record.LecturerId} conflicts with another row in this CSV",
                            Code = $"BLC:{record.LecturerId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var batchGroupConflicts = batchSchedules
                        .Where(b => b.Date.Date == record.ScheduleDate.Date &&
                                    b.TimeTableId == record.TimeTableId &&
                                    b.WeekType == record.WeekType)
                        .SelectMany(b => b.GroupIds)
                        .Intersect(groupIds)
                        .ToList();

                    if (batchGroupConflicts.Any())
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Groups {string.Join(", ", batchGroupConflicts)} conflict with another row in this CSV",
                            Code = $"BGC:{string.Join("-", batchGroupConflicts)}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // All validations passed - create schedule with groups
                    var schedule = new Schedule
                    {
                        SubjectId = record.SubjectId,
                        LecturerId = record.LecturerId,
                        LessonTypeId = record.LessonType,
                        ClassroomId = record.ClassroomId,
                        TimeTableId = record.TimeTableId,
                        WeekType = record.WeekType,
                        ScheduleDate = record.ScheduleDate,
                        SemesterId = record.SemesterId,
                        ScheduleParentId = record.ScheduleParentId,
                        ScheduleGroups = groupIds.Select(gId => new ScheduleGroup { GroupId = gId }).ToList()
                    };

                    schedulesToAdd.Add(schedule);
                    batchSchedules.Add((record.ScheduleDate, record.TimeTableId, record.ClassroomId, record.LecturerId, record.WeekType, groupIds));
                    result.SuccessCount++;
                }

                if (schedulesToAdd.Any())
                {
                    await _context.Schedules.AddRangeAsync(schedulesToAdd, ct);
                    await _context.SaveChangesAsync(ct);
                }

                return Result.Success(result, $"Bulk upload completed. Success: {result.SuccessCount}, Failed: {result.FailureCount}");
            }
            catch (Exception ex)
            {
                return Result.Failure<BulkUploadResult>(
                    ErrorType.Failure,
                    $"Error parsing CSV: {ex.Message}");
            }
        }

        private ValidationResult ValidateRecord(CsvScheduleRecord record)
        {
            var validator = new CsvScheduleValidator();
            return validator.Validate(record);
        }

        private List<int> ParseGroupIds(string groupIdsString)
        {
            if (string.IsNullOrWhiteSpace(groupIdsString))
                return new List<int>();

            return groupIdsString
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
        }
    }
}
