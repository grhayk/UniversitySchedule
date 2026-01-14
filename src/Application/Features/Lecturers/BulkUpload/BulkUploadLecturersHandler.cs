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

namespace Application.Features.Lecturers.BulkUpload
{
    public class BulkUploadLecturersHandler : IRequestHandler<BulkUploadLecturersCommand, Result<BulkUploadResult>>
    {
        private readonly IDbContext _dbContext;

        public BulkUploadLecturersHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<BulkUploadResult>> Handle(BulkUploadLecturersCommand request, CancellationToken ct)
        {
            var result = new BulkUploadResult();
            var lecturersToAdd = new List<Lecturer>();

            try
            {
                using var reader = new StringReader(request.CsvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<CsvLecturerMap>();
                var csvRecords = csv.GetRecords<CsvLecturerRecord>().ToList();

                // Get valid structure IDs
                var validStructureIds = await _dbContext.Structures
                    .Select(s => s.Id)
                    .ToListAsync(ct);

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

                    // Validate structure exists
                    if (!validStructureIds.Contains(record.StructureId))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Structure with ID {record.StructureId} not found",
                            Code = $"{record.FirstName} {record.LastName}"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    var lecturer = new Lecturer
                    {
                        FirstName = record.FirstName,
                        LastName = record.LastName,
                        BirthDate = record.BirthDate,
                        StructureId = record.StructureId
                    };

                    lecturersToAdd.Add(lecturer);
                    result.SuccessCount++;
                }

                if (lecturersToAdd.Any())
                {
                    await _dbContext.Lecturers.AddRangeAsync(lecturersToAdd, ct);
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

        private ValidationResult ValidateRecord(CsvLecturerRecord record)
        {
            var validator = new CsvLecturerValidator();
            return validator.Validate(record);
        }
    }
}
