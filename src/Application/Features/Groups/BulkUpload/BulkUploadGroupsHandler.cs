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

namespace Application.Features.Groups.BulkUpload
{
    public class BulkUploadGroupsHandler : IRequestHandler<BulkUploadGroupsCommand, Result<BulkUploadResult>>
    {
        private readonly IDbContext _dbContext;

        public BulkUploadGroupsHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<BulkUploadResult>> Handle(BulkUploadGroupsCommand request, CancellationToken ct)
        {
            var result = new BulkUploadResult();
            var groupsToAdd = new List<Group>();

            try
            {
                using var reader = new StringReader(request.CsvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<CsvGroupMap>();
                var csvRecords = csv.GetRecords<CsvGroupRecord>().ToList();

                // Get valid IDs for validation
                var validProgramIds = await _dbContext.EducationPrograms.Select(p => p.Id).ToListAsync(ct);
                var validSemesterIds = await _dbContext.Semesters.Select(s => s.Id).ToListAsync(ct);
                var existingGroupIds = await _dbContext.Groups.Select(g => g.Id).ToListAsync(ct);

                // Get existing groups for duplicate checking
                var existingGroups = await _dbContext.Groups
                    .Select(g => new { g.EducationProgramId, g.SemesterId, g.LessonType, g.IndexNumber, g.ParentId })
                    .ToListAsync(ct);

                // Track groups being added in this batch
                var groupsInBatch = new HashSet<(int, int, LessonType, int, int?)>();

                foreach (var (record, rowNumber) in csvRecords.Select((r, i) => (r, i + 2)))
                {
                    var validationResult = ValidateRecord(record);
                    if (!validationResult.IsValid)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                            Code = $"Program:{record.EducationProgramId}-Sem:{record.SemesterId}-{record.LessonType}-{record.IndexNumber}"
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
                            Code = $"Program:{record.EducationProgramId}"
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
                            Code = $"Semester:{record.SemesterId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate parent group exists if specified
                    if (record.ParentId.HasValue && !existingGroupIds.Contains(record.ParentId.Value))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Parent group with ID {record.ParentId} not found",
                            Code = $"Parent:{record.ParentId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate branched from group exists if specified
                    if (record.BranchedFromGroupId.HasValue && !existingGroupIds.Contains(record.BranchedFromGroupId.Value))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Branched from group with ID {record.BranchedFromGroupId} not found",
                            Code = $"BranchedFrom:{record.BranchedFromGroupId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var groupKey = (record.EducationProgramId, record.SemesterId, record.LessonType, record.IndexNumber, record.ParentId);

                    // Check for duplicate in existing groups
                    if (existingGroups.Any(g => g.EducationProgramId == record.EducationProgramId
                                              && g.SemesterId == record.SemesterId
                                              && g.LessonType == record.LessonType
                                              && g.IndexNumber == record.IndexNumber
                                              && g.ParentId == record.ParentId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = "Group with same program, semester, lesson type, index, and parent already exists",
                            Code = $"Duplicate"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check for duplicate in current batch
                    if (groupsInBatch.Contains(groupKey))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = "Duplicate group found in CSV",
                            Code = $"Duplicate in batch"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var group = new Group
                    {
                        ParentId = record.ParentId,
                        EducationProgramId = record.EducationProgramId,
                        SemesterId = record.SemesterId,
                        LessonType = record.LessonType,
                        IsActive = record.IsActive,
                        StartDate = record.StartDate,
                        IndexNumber = record.IndexNumber,
                        BranchedFromGroupId = record.BranchedFromGroupId
                    };

                    groupsToAdd.Add(group);
                    groupsInBatch.Add(groupKey);
                    result.SuccessCount++;
                }

                if (groupsToAdd.Any())
                {
                    await _dbContext.Groups.AddRangeAsync(groupsToAdd, ct);
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

        private ValidationResult ValidateRecord(CsvGroupRecord record)
        {
            var validator = new CsvGroupValidator();
            return validator.Validate(record);
        }
    }
}
