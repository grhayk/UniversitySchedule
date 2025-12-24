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

namespace Application.Features.EducationPrograms.BulkUpload
{
    public class BulkUploadEducationProgramsHandler : IRequestHandler<BulkUploadEducationProgramsCommand, Result<BulkUploadResult>>
    {
        private readonly IDbContext _dbContext;

        public BulkUploadEducationProgramsHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<BulkUploadResult>> Handle(BulkUploadEducationProgramsCommand request, CancellationToken ct)
        {
            var result = new BulkUploadResult();
            var records = new List<EducationProgram>();

            try
            {
                using var reader = new StringReader(request.CsvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<CsvEducationProgramMap>();
                var csvRecords = csv.GetRecords<CsvEducationProgramRecord>().ToList();

                // Validate all records first
                foreach (var (record, rowNumber) in csvRecords.Select((r, i) => (r, i + 2)))
                {
                    var validationResult = ValidateRecord(record);
                    if (!validationResult.IsValid)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = string.Join("; ", validationResult.Errors),
                            Code = record.Code
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Validate structure exists and is of type Chair
                    var structure = await _dbContext.Structures.FirstOrDefaultAsync(s => s.Id == record.StructureId, ct);

                    if (structure is null || structure.Type != StructureType.Chair)
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Structure with ID {record.StructureId} not found or is not chair",
                            Code = record.Code
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check if parent exists (if provided)
                    if (record.ParentId.HasValue)
                    {
                        var parentExists = await _dbContext.EducationPrograms.AnyAsync(p => p.Id == record.ParentId.Value, ct);

                        if (!parentExists)
                        {
                            result.Errors.Add(new BulkUploadError
                            {
                                RowNumber = rowNumber,
                                Message = $"Parent Program with ID {record.ParentId} not found",
                                Code = record.Code
                            });
                            result.FailureCount++;
                            continue;
                        }
                    }

                    records.Add(new EducationProgram
                    {
                        Code = record.Code,
                        Name = record.Name,
                        StructureId = record.StructureId,
                        ParentId = record.ParentId
                    });

                    result.SuccessCount++;
                }

                // Bulk insert valid records
                if (records.Any())
                {
                    await _dbContext.EducationPrograms.AddRangeAsync(records, ct);
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

        private ValidationResult ValidateRecord(CsvEducationProgramRecord record)
        {
            var validator = new CsvEducationProgramValidator();
            return validator.Validate(record);
        }
    }
}
