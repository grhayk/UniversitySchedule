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

namespace Application.Features.SubjectClassrooms.BulkUpload
{
    public class BulkUploadSubjectClassroomsHandler : IRequestHandler<BulkUploadSubjectClassroomsCommand, Result<BulkUploadResult>>
    {
        private readonly IDbContext _dbContext;

        public BulkUploadSubjectClassroomsHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<BulkUploadResult>> Handle(BulkUploadSubjectClassroomsCommand request, CancellationToken ct)
        {
            var result = new BulkUploadResult();
            var itemsToAdd = new List<SubjectClassroom>();

            try
            {
                using var reader = new StringReader(request.CsvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<CsvSubjectClassroomMap>();
                var csvRecords = csv.GetRecords<CsvSubjectClassroomRecord>().ToList();

                // Get subjects with their configs
                var subjectIds = csvRecords.Select(r => r.SubjectId).Distinct().ToList();
                var subjects = await _dbContext.Subjects
                    .Include(s => s.SubjectConfigs)
                    .Where(s => subjectIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, ct);

                // Get valid classroom IDs
                var classroomIds = csvRecords.Select(r => r.ClassroomId).Distinct().ToList();
                var validClassroomIds = await _dbContext.Classrooms
                    .Where(c => classroomIds.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync(ct);

                // Get existing assignments for duplicate check
                var existingAssignments = await _dbContext.SubjectClassrooms
                    .Where(sc => subjectIds.Contains(sc.SubjectId))
                    .Select(sc => new { sc.SubjectId, sc.LessonType, sc.ClassroomId })
                    .ToListAsync(ct);

                // Track assignments in batch
                var assignmentsInBatch = new HashSet<(int, LessonType, int)>();

                foreach (var (record, rowNumber) in csvRecords.Select((r, i) => (r, i + 2)))
                {
                    var validationResult = ValidateRecord(record);
                    if (!validationResult.IsValid)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                            Code = $"Subject:{record.SubjectId}-{record.LessonType}-Classroom:{record.ClassroomId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate subject exists
                    if (!subjects.TryGetValue(record.SubjectId, out var subject))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Subject with ID {record.SubjectId} not found",
                            Code = $"Subject:{record.SubjectId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate subject has this lesson type
                    if (!subject.SubjectConfigs.Any(c => c.LessonType == record.LessonType))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Subject does not have {record.LessonType} configured",
                            Code = $"Subject:{record.SubjectId}-{record.LessonType}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate classroom exists
                    if (!validClassroomIds.Contains(record.ClassroomId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Classroom with ID {record.ClassroomId} not found",
                            Code = $"Classroom:{record.ClassroomId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var assignmentKey = (record.SubjectId, record.LessonType, record.ClassroomId);

                    // Check for duplicate in existing
                    if (existingAssignments.Any(a => a.SubjectId == record.SubjectId
                                                   && a.LessonType == record.LessonType
                                                   && a.ClassroomId == record.ClassroomId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = "Assignment already exists in database",
                            Code = $"Subject:{record.SubjectId}-{record.LessonType}-Classroom:{record.ClassroomId}"
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
                            Code = $"Subject:{record.SubjectId}-{record.LessonType}-Classroom:{record.ClassroomId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var subjectClassroom = new SubjectClassroom
                    {
                        SubjectId = record.SubjectId,
                        LessonType = record.LessonType,
                        ClassroomId = record.ClassroomId
                    };

                    itemsToAdd.Add(subjectClassroom);
                    assignmentsInBatch.Add(assignmentKey);
                    result.SuccessCount++;
                }

                if (itemsToAdd.Any())
                {
                    await _dbContext.SubjectClassrooms.AddRangeAsync(itemsToAdd, ct);
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

        private ValidationResult ValidateRecord(CsvSubjectClassroomRecord record)
        {
            var validator = new CsvSubjectClassroomValidator();
            return validator.Validate(record);
        }
    }
}
