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

namespace Application.Features.StudentGroups.BulkUpload
{
    public class BulkUploadStudentGroupsHandler : IRequestHandler<BulkUploadStudentGroupsCommand, Result<BulkUploadResult>>
    {
        private readonly IDbContext _dbContext;

        public BulkUploadStudentGroupsHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<BulkUploadResult>> Handle(BulkUploadStudentGroupsCommand request, CancellationToken ct)
        {
            var result = new BulkUploadResult();
            var studentGroupsToAdd = new List<StudentGroup>();

            try
            {
                using var reader = new StringReader(request.CsvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<CsvStudentGroupMap>();
                var csvRecords = csv.GetRecords<CsvStudentGroupRecord>().ToList();

                // Get students with their parent group
                var studentIds = csvRecords.Select(r => r.StudentId).Distinct().ToList();
                var students = await _dbContext.Students
                    .Where(s => studentIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, ct);

                // Get groups with semester info
                var groupIds = csvRecords.Select(r => r.GroupId).Distinct().ToList();
                var groups = await _dbContext.Groups
                    .Where(g => groupIds.Contains(g.Id))
                    .ToDictionaryAsync(g => g.Id, ct);

                // Get existing assignments to check for duplicates
                var existingAssignments = await _dbContext.StudentGroups
                    .Where(sg => studentIds.Contains(sg.StudentId))
                    .Select(sg => new { sg.StudentId, sg.GroupId })
                    .ToListAsync(ct);

                // Track assignments in current batch
                var assignmentsInBatch = new HashSet<(int, int)>();

                foreach (var (record, rowNumber) in csvRecords.Select((r, i) => (r, i + 2)))
                {
                    var validationResult = ValidateRecord(record);
                    if (!validationResult.IsValid)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                            Code = $"Student:{record.StudentId}-Group:{record.GroupId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate student exists
                    if (!students.TryGetValue(record.StudentId, out var student))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Student with ID {record.StudentId} not found",
                            Code = $"Student:{record.StudentId}-Group:{record.GroupId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate group exists
                    if (!groups.TryGetValue(record.GroupId, out var group))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Group with ID {record.GroupId} not found",
                            Code = $"Student:{record.StudentId}-Group:{record.GroupId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate student belongs to parent group
                    var parentGroupId = group.ParentId ?? group.Id;
                    if (student.GroupId != parentGroupId)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Student does not belong to parent group {parentGroupId}",
                            Code = $"Student:{record.StudentId}-Group:{record.GroupId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var assignmentKey = (record.StudentId, record.GroupId);

                    // Check for duplicate in existing assignments
                    if (existingAssignments.Any(a => a.StudentId == record.StudentId && a.GroupId == record.GroupId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = "Assignment already exists in database",
                            Code = $"Student:{record.StudentId}-Group:{record.GroupId}"
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
                            Code = $"Student:{record.StudentId}-Group:{record.GroupId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var studentGroup = new StudentGroup
                    {
                        StudentId = record.StudentId,
                        GroupId = record.GroupId,
                        SemesterId = group.SemesterId
                    };

                    studentGroupsToAdd.Add(studentGroup);
                    assignmentsInBatch.Add(assignmentKey);
                    result.SuccessCount++;
                }

                if (studentGroupsToAdd.Any())
                {
                    await _dbContext.StudentGroups.AddRangeAsync(studentGroupsToAdd, ct);
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

        private ValidationResult ValidateRecord(CsvStudentGroupRecord record)
        {
            var validator = new CsvStudentGroupValidator();
            return validator.Validate(record);
        }
    }
}
