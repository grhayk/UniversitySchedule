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

namespace Application.Features.EducationProgramSubjects.BulkUpload
{
    public class BulkUploadProgramSubjectsHandler : IRequestHandler<BulkUploadProgramSubjectsCommand, Result<BulkUploadResult>>
    {
        private readonly IDbContext _dbContext;

        public BulkUploadProgramSubjectsHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<BulkUploadResult>> Handle(BulkUploadProgramSubjectsCommand request, CancellationToken ct)
        {
            var result = new BulkUploadResult();
            var itemsToAdd = new List<EducationProgramSubject>();

            try
            {
                using var reader = new StringReader(request.CsvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<CsvProgramSubjectMap>();
                var csvRecords = csv.GetRecords<CsvProgramSubjectRecord>().ToList();

                // Get existing assignments for duplicate checking
                var existingAssignments = await _dbContext.EducationProgramSubjects
                    .Select(eps => new { eps.EducationProgramId, eps.SubjectId, eps.SemesterId })
                    .ToListAsync(ct);

                // Track assignments being added in this batch
                var assignmentsInBatch = new HashSet<(int, int, int)>();

                // Get valid IDs for validation
                var validProgramIds = await _dbContext.EducationPrograms.Select(p => p.Id).ToListAsync(ct);
                var validSubjectIds = await _dbContext.Subjects.Select(s => s.Id).ToListAsync(ct);
                var validSemesterIds = await _dbContext.Semesters.Select(s => s.Id).ToListAsync(ct);

                foreach (var (record, rowNumber) in csvRecords.Select((r, i) => (r, i + 2)))
                {
                    var validationResult = ValidateRecord(record);
                    if (!validationResult.IsValid)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                            Code = $"Program:{record.EducationProgramId}-Subject:{record.SubjectId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate education program exists
                    if (!validProgramIds.Contains(record.EducationProgramId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Education program with ID {record.EducationProgramId} not found",
                            Code = $"Program:{record.EducationProgramId}-Subject:{record.SubjectId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate subject exists
                    if (!validSubjectIds.Contains(record.SubjectId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Subject with ID {record.SubjectId} not found",
                            Code = $"Program:{record.EducationProgramId}-Subject:{record.SubjectId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate semester exists
                    if (!validSemesterIds.Contains(record.SemesterId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Semester with ID {record.SemesterId} not found",
                            Code = $"Program:{record.EducationProgramId}-Subject:{record.SubjectId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var assignmentKey = (record.EducationProgramId, record.SubjectId, record.SemesterId);

                    // Check for duplicate in existing assignments
                    if (existingAssignments.Any(a => a.EducationProgramId == record.EducationProgramId
                                                   && a.SubjectId == record.SubjectId
                                                   && a.SemesterId == record.SemesterId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Assignment already exists in database",
                            Code = $"Program:{record.EducationProgramId}-Subject:{record.SubjectId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check for duplicate in current batch
                    if (assignmentsInBatch.Contains(assignmentKey))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Duplicate assignment found in CSV",
                            Code = $"Program:{record.EducationProgramId}-Subject:{record.SubjectId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var programSubject = new EducationProgramSubject
                    {
                        EducationProgramId = record.EducationProgramId,
                        SubjectId = record.SubjectId,
                        SemesterId = record.SemesterId,
                        FromDate = record.FromDate,
                        ToDate = record.ToDate
                    };

                    itemsToAdd.Add(programSubject);
                    assignmentsInBatch.Add(assignmentKey);
                    result.SuccessCount++;
                }

                if (itemsToAdd.Any())
                {
                    await _dbContext.EducationProgramSubjects.AddRangeAsync(itemsToAdd, ct);
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

        private ValidationResult ValidateRecord(CsvProgramSubjectRecord record)
        {
            var validator = new CsvProgramSubjectValidator();
            return validator.Validate(record);
        }
    }
}
