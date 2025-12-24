using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EducationPrograms.UpdateEducationProgram
{
    public class UpdateEducationProgramHandler : IRequestHandler<UpdateEducationProgramCommand, Result>
    {
        private readonly IDbContext _dbContext;

        public UpdateEducationProgramHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result> Handle(UpdateEducationProgramCommand request, CancellationToken ct)
        {
            var program = await _dbContext.EducationPrograms.FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (program == null)
            {
                return Result.Failure(ErrorType.NotFound, $"Education Program with ID {request.Id} not found");
            }

            // Validate structure exists and is of type Chair
            var structure = await _dbContext.Structures.FirstOrDefaultAsync(s => s.Id == request.StructureId, ct);

            if (structure is null || structure.Type != StructureType.Chair)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Structure with ID {request.StructureId} not found or is not chair.");
            }

            // Validate parent exists (if provided)
            if (request.ParentId.HasValue)
            {
                var parentExists = await _dbContext.EducationPrograms.AnyAsync(p => p.Id == request.ParentId.Value, ct);

                if (!parentExists)
                {
                    return Result.Failure(ErrorType.NotFound, $"Parent Education Program with ID {request.ParentId} not found");
                }
            }

            // Update
            program.Code = request.Code;
            program.Name = request.Name;
            program.StructureId = request.StructureId;
            program.ParentId = request.ParentId;

            _dbContext.EducationPrograms.Update(program);
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success("Education Program updated successfully");
        }
    }
}
