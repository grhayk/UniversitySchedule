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

namespace Application.Features.Subjects.BulkUpload
{
    public class BulkUploadSubjectsHandler : IRequestHandler<BulkUploadSubjectsCommand, Result<BulkUploadResult>>
    {
        private readonly IDbContext _dbContext;

        public BulkUploadSubjectsHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<BulkUploadResult>> Handle(BulkUploadSubjectsCommand request, CancellationToken ct)
        {
            var result = new BulkUploadResult();
            var subjectsToAdd = new List<Subject>();

            try
            {
                using var reader = new StringReader(request.CsvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<CsvSubjectMap>();
                var csvRecords = csv.GetRecords<CsvSubjectRecord>().ToList();

                // Get all existing subject codes for duplicate checking
                var existingSubjectCodes = await _dbContext.Subjects
                    .Select(s => s.Code)
                    .ToListAsync(ct);

                // Track codes being added in this batch
                var codesInBatch = new HashSet<string>();

                // Get all valid semester IDs
                var validSemesterIds = await _dbContext.Semesters
                    .Select(s => s.Id)
                    .ToListAsync(ct);

                // Validate all records first
                foreach (var (record, rowNumber) in csvRecords.Select((r, i) => (r, i + 2)))
                {
                    var validationResult = ValidateRecord(record);
                    if (!validationResult.IsValid)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                            Code = record.Code
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check for duplicate code in existing subjects
                    if (existingSubjectCodes.Contains(record.Code))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Subject with code '{record.Code}' already exists in database",
                            Code = record.Code
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check for duplicate code in current batch
                    if (codesInBatch.Contains(record.Code))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Duplicate subject code '{record.Code}' found in CSV",
                            Code = record.Code
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate structure exists and is of type Chair
                    var structure = await _dbContext.Structures.FirstOrDefaultAsync(s => s.Id == record.StructureId, ct);

                    if (structure is null)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Structure with ID {record.StructureId} not found",
                            Code = record.Code
                        });
                        result.FailureCount++;
                        continue;
                    }

                    if (structure.Type != StructureType.Chair)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Structure with ID {record.StructureId} is not a Chair (Type: {structure.Type})",
                            Code = record.Code
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate SemesterIdFrom exists
                    if (!validSemesterIds.Contains(record.SemesterIdFrom))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"SemesterFrom with ID {record.SemesterIdFrom} not found",
                            Code = record.Code
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate SemesterIdTo exists
                    if (!validSemesterIds.Contains(record.SemesterIdTo))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"SemesterTo with ID {record.SemesterIdTo} not found",
                            Code = record.Code
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Create subject with configs
                    var subject = new Subject
                    {
                        Code = record.Code,
                        Name = record.Name,
                        SemesterIdFrom = record.SemesterIdFrom,
                        SemesterIdTo = record.SemesterIdTo,
                        StructureId = record.StructureId,
                        SubjectConfigs = new List<SubjectConfig>()
                    };

                    // Add configs based on hours
                    if (record.LectureHours.HasValue && record.LectureHours > 0)
                    {
                        subject.SubjectConfigs.Add(new SubjectConfig
                        {
                            LessonType = LessonType.Lecture,
                            Hours = record.LectureHours.Value
                        });
                    }

                    if (record.PracticalHours.HasValue && record.PracticalHours > 0)
                    {
                        subject.SubjectConfigs.Add(new SubjectConfig
                        {
                            LessonType = LessonType.Practical,
                            Hours = record.PracticalHours.Value
                        });
                    }

                    if (record.LaboratoryHours.HasValue && record.LaboratoryHours > 0)
                    {
                        subject.SubjectConfigs.Add(new SubjectConfig
                        {
                            LessonType = LessonType.Laboratory,
                            Hours = record.LaboratoryHours.Value
                        });
                    }

                    subjectsToAdd.Add(subject);
                    codesInBatch.Add(record.Code);
                    result.SuccessCount++;
                }

                // Bulk insert valid records
                if (subjectsToAdd.Any())
                {
                    await _dbContext.Subjects.AddRangeAsync(subjectsToAdd, ct);
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

        private ValidationResult ValidateRecord(CsvSubjectRecord record)
        {
            var validator = new CsvSubjectValidator();
            return validator.Validate(record);
        }
    }
}
