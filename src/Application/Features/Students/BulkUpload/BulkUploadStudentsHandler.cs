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

namespace Application.Features.Students.BulkUpload
{
    public class BulkUploadStudentsHandler : IRequestHandler<BulkUploadStudentsCommand, Result<BulkUploadResult>>
    {
        private readonly IDbContext _dbContext;

        public BulkUploadStudentsHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<BulkUploadResult>> Handle(BulkUploadStudentsCommand request, CancellationToken ct)
        {
            var result = new BulkUploadResult();
            var studentsToAdd = new List<Student>();
            var studentGroupsToAdd = new List<StudentGroup>();

            try
            {
                using var reader = new StringReader(request.CsvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<CsvStudentMap>();
                var csvRecords = csv.GetRecords<CsvStudentRecord>().ToList();

                // Get groups with their related data for deriving fields (only parent groups)
                var groupIds = csvRecords.Select(r => r.GroupId).Distinct().ToList();
                var groups = await _dbContext.Groups
                    .Include(g => g.EducationProgram)
                    .Include(g => g.Semester)
                    .Where(g => groupIds.Contains(g.Id))
                    .ToDictionaryAsync(g => g.Id, ct);

                foreach (var (record, rowNumber) in csvRecords.Select((r, i) => (r, i + 2)))
                {
                    var validationResult = ValidateRecord(record);
                    if (!validationResult.IsValid)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                            Code = $"{record.FirstName} {record.LastName}"
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
                            Code = $"{record.FirstName} {record.LastName}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Group must be a parent group (lecture group)
                    if (group.ParentId != null)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = "Student can only be assigned to a parent group (lecture group)",
                            Code = $"{record.FirstName} {record.LastName}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var student = new Student
                    {
                        FirstName = record.FirstName,
                        LastName = record.LastName,
                        BirthDate = record.BirthDate,
                        GroupId = record.GroupId,
                        // Derived from Group's relationships
                        StructureId = group.EducationProgram.StructureId,
                        EducationDegree = group.Semester.EducationDegree,
                        EducationType = group.Semester.EducationType
                    };

                    // Also create StudentGroup record
                    var studentGroup = new StudentGroup
                    {
                        Student = student,
                        GroupId = record.GroupId,
                        SemesterId = group.SemesterId
                    };

                    studentsToAdd.Add(student);
                    studentGroupsToAdd.Add(studentGroup);
                    result.SuccessCount++;
                }

                if (studentsToAdd.Any())
                {
                    await _dbContext.Students.AddRangeAsync(studentsToAdd, ct);
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

        private ValidationResult ValidateRecord(CsvStudentRecord record)
        {
            var validator = new CsvStudentValidator();
            return validator.Validate(record);
        }
    }
}
