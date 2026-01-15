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

namespace Application.Features.LecturerSubjects.BulkUpload
{
    public class BulkUploadLecturerSubjectsHandler : IRequestHandler<BulkUploadLecturerSubjectsCommand, Result<BulkUploadResult>>
    {
        private readonly IDbContext _dbContext;

        public BulkUploadLecturerSubjectsHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<BulkUploadResult>> Handle(BulkUploadLecturerSubjectsCommand request, CancellationToken ct)
        {
            var result = new BulkUploadResult();
            var itemsToAdd = new List<LecturerSubject>();

            try
            {
                using var reader = new StringReader(request.CsvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<CsvLecturerSubjectMap>();
                var csvRecords = csv.GetRecords<CsvLecturerSubjectRecord>().ToList();

                // Get valid lecturer and subject IDs
                var lecturerIds = csvRecords.Select(r => r.LecturerId).Distinct().ToList();
                var validLecturerIds = await _dbContext.Lecturers
                    .Where(l => lecturerIds.Contains(l.Id))
                    .Select(l => l.Id)
                    .ToListAsync(ct);

                var subjectIds = csvRecords.Select(r => r.SubjectId).Distinct().ToList();
                var validSubjectIds = await _dbContext.Subjects
                    .Where(s => subjectIds.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync(ct);

                // Get existing assignments
                var existingAssignments = await _dbContext.LecturerSubjects
                    .Where(ls => lecturerIds.Contains(ls.LecturerId))
                    .Select(ls => new { ls.LecturerId, ls.SubjectId })
                    .ToListAsync(ct);

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
                            Code = $"Lecturer:{record.LecturerId}-Subject:{record.SubjectId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    if (!validLecturerIds.Contains(record.LecturerId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Lecturer with ID {record.LecturerId} not found",
                            Code = $"Lecturer:{record.LecturerId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    if (!validSubjectIds.Contains(record.SubjectId))
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

                    var assignmentKey = (record.LecturerId, record.SubjectId);

                    if (existingAssignments.Any(a => a.LecturerId == record.LecturerId && a.SubjectId == record.SubjectId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = "Assignment already exists in database",
                            Code = $"Lecturer:{record.LecturerId}-Subject:{record.SubjectId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    if (assignmentsInBatch.Contains(assignmentKey))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = "Duplicate assignment in CSV",
                            Code = $"Lecturer:{record.LecturerId}-Subject:{record.SubjectId}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var lecturerSubject = new LecturerSubject
                    {
                        LecturerId = record.LecturerId,
                        SubjectId = record.SubjectId
                    };

                    itemsToAdd.Add(lecturerSubject);
                    assignmentsInBatch.Add(assignmentKey);
                    result.SuccessCount++;
                }

                if (itemsToAdd.Any())
                {
                    await _dbContext.LecturerSubjects.AddRangeAsync(itemsToAdd, ct);
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

        private ValidationResult ValidateRecord(CsvLecturerSubjectRecord record)
        {
            var validator = new CsvLecturerSubjectValidator();
            return validator.Validate(record);
        }
    }
}
