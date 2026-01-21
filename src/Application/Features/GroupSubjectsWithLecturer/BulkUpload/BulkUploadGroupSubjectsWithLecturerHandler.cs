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

namespace Application.Features.GroupSubjectsWithLecturer.BulkUpload
{
    public class BulkUploadGroupSubjectsWithLecturerHandler : IRequestHandler<BulkUploadGroupSubjectsWithLecturerCommand, Result<BulkUploadResult>>
    {
        private readonly IDbContext _dbContext;

        public BulkUploadGroupSubjectsWithLecturerHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<BulkUploadResult>> Handle(BulkUploadGroupSubjectsWithLecturerCommand request, CancellationToken ct)
        {
            var result = new BulkUploadResult();
            var itemsToAdd = new List<GroupSubjectWithLecturer>();

            try
            {
                using var reader = new StringReader(request.CsvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<CsvGroupSubjectWithLecturerMap>();
                var csvRecords = csv.GetRecords<CsvGroupSubjectWithLecturerRecord>().ToList();

                // Get LecturerSubjects with Subject and SubjectConfigs
                var lecturerSubjectIds = csvRecords.Select(r => r.LecturerSubjectId).Distinct().ToList();
                var lecturerSubjects = await _dbContext.LecturerSubjects
                    .Include(ls => ls.Subject)
                        .ThenInclude(s => s.SubjectConfigs)
                    .Where(ls => lecturerSubjectIds.Contains(ls.Id))
                    .ToDictionaryAsync(ls => ls.Id, ct);

                // Get valid group IDs
                var groupIds = csvRecords.Select(r => r.GroupId).Distinct().ToList();
                var validGroupIds = await _dbContext.Groups
                    .Where(g => groupIds.Contains(g.Id))
                    .Select(g => g.Id)
                    .ToListAsync(ct);

                // Get existing assignments
                var existingAssignments = await _dbContext.GroupSubjectsWithLecturer
                    .Where(g => lecturerSubjectIds.Contains(g.LecturerSubjectId))
                    .Select(g => new { g.LecturerSubjectId, g.GroupId, g.LessonType })
                    .ToListAsync(ct);

                var assignmentsInBatch = new HashSet<(int, int, LessonType)>();

                foreach (var (record, rowNumber) in csvRecords.Select((r, i) => (r, i + 2)))
                {
                    var validationResult = ValidateRecord(record);
                    if (!validationResult.IsValid)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                            Code = $"LS:{record.LecturerSubjectId}-G:{record.GroupId}-{record.LessonType}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate LecturerSubject exists
                    if (!lecturerSubjects.TryGetValue(record.LecturerSubjectId, out var lecturerSubject))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"LecturerSubject with ID {record.LecturerSubjectId} not found",
                            Code = $"LS:{record.LecturerSubjectId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate group exists
                    if (!validGroupIds.Contains(record.GroupId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Group with ID {record.GroupId} not found",
                            Code = $"G:{record.GroupId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Get SubjectConfig for this LessonType
                    var subjectConfig = lecturerSubject.Subject.SubjectConfigs
                        .FirstOrDefault(c => c.LessonType == record.LessonType);

                    if (subjectConfig is null)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Subject does not have {record.LessonType} configured",
                            Code = $"LS:{record.LecturerSubjectId}-{record.LessonType}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var assignmentKey = (record.LecturerSubjectId, record.GroupId, record.LessonType);

                    // Check for duplicate in existing
                    if (existingAssignments.Any(a => a.LecturerSubjectId == record.LecturerSubjectId
                                                   && a.GroupId == record.GroupId
                                                   && a.LessonType == record.LessonType))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = "Assignment already exists in database",
                            Code = $"LS:{record.LecturerSubjectId}-G:{record.GroupId}-{record.LessonType}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check for duplicate in batch
                    if (assignmentsInBatch.Contains(assignmentKey))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = "Duplicate assignment in CSV",
                            Code = $"LS:{record.LecturerSubjectId}-G:{record.GroupId}-{record.LessonType}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var item = new GroupSubjectWithLecturer
                    {
                        LecturerSubjectId = record.LecturerSubjectId,
                        GroupId = record.GroupId,
                        LessonType = record.LessonType,
                        Hours = subjectConfig.Hours // Derived from SubjectConfig
                    };

                    itemsToAdd.Add(item);
                    assignmentsInBatch.Add(assignmentKey);
                    result.SuccessCount++;
                }

                if (itemsToAdd.Any())
                {
                    await _dbContext.GroupSubjectsWithLecturer.AddRangeAsync(itemsToAdd, ct);
                    await _dbContext.SaveChangesAsync(ct);
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

        private ValidationResult ValidateRecord(CsvGroupSubjectWithLecturerRecord record)
        {
            var validator = new CsvGroupSubjectWithLecturerValidator();
            return validator.Validate(record);
        }
    }
}
