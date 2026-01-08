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

namespace Application.Features.Classrooms.BulkUpload
{
    public class BulkUploadClassroomsHandler : IRequestHandler<BulkUploadClassroomsCommand, Result<BulkUploadResult>>
    {
        private readonly IDbContext _dbContext;

        public BulkUploadClassroomsHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<BulkUploadResult>> Handle(BulkUploadClassroomsCommand request, CancellationToken ct)
        {
            var result = new BulkUploadResult();
            var classroomsToAdd = new List<Classroom>();

            try
            {
                using var reader = new StringReader(request.CsvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<CsvClassroomMap>();
                var csvRecords = csv.GetRecords<CsvClassroomRecord>().ToList();

                // Get all existing classroom names for duplicate checking
                var existingClassroomNames = await _dbContext.Classrooms
                    .Select(c => c.Name)
                    .ToListAsync(ct);

                // Track names being added in this batch
                var namesInBatch = new HashSet<string>();

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
                            Code = record.Name
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check for duplicate name in existing classrooms
                    if (existingClassroomNames.Contains(record.Name))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Classroom with name '{record.Name}' already exists in database",
                            Code = record.Name
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check for duplicate name in current batch
                    if (namesInBatch.Contains(record.Name))
                    {
                        result.Errors.Add(new BulkUploadError
                        {
                            RowNumber = rowNumber,
                            Message = $"Duplicate classroom name '{record.Name}' found in CSV",
                            Code = record.Name
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
                            Code = record.Name
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
                            Code = record.Name
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Create classroom with characteristics
                    var classroom = new Classroom
                    {
                        Name = record.Name,
                        StructureId = record.StructureId,
                        Characteristics = new ClassroomCharacteristics
                        {
                            Type = record.Type,
                            SeatCapacity = record.SeatCapacity,
                            HasComputer = record.HasComputer,
                            ComputerCount = record.ComputerCount,
                            HasProjector = record.HasProjector,
                            RenovationStatus = record.RenovationStatus,
                            BlackboardCondition = record.BlackboardCondition
                        }
                    };

                    classroomsToAdd.Add(classroom);
                    namesInBatch.Add(record.Name);
                    result.SuccessCount++;
                }

                // Bulk insert valid records
                if (classroomsToAdd.Any())
                {
                    await _dbContext.Classrooms.AddRangeAsync(classroomsToAdd, ct);
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

        private ValidationResult ValidateRecord(CsvClassroomRecord record)
        {
            var validator = new CsvClassroomValidator();
            return validator.Validate(record);
        }
    }
}
