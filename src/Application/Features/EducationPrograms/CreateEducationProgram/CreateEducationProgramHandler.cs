using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EducationPrograms.CreateEducationProgram
{
    internal class CreateEducationProgramHandler : IRequestHandler<CreateEducationProgramCommand, Result<int>>
    {
        private readonly IDbContext _context;
        public CreateEducationProgramHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(CreateEducationProgramCommand request, CancellationToken ct)
        {
            // If ParentId is provided, does it exist?
            if (request.ParentId.HasValue)
            {
                var parentExists = await _context.EducationPrograms.AnyAsync(p => p.Id == request.ParentId.Value, ct);

                if (!parentExists)
                {
                    return Result.Failure<int>(ErrorType.NotFound, $"Parent Education Program with ID {request.ParentId} was not found.");
                }
            }

            // Validate structure exists and is of type Chair
            var structure = await _context.Structures.FirstOrDefaultAsync(s => s.Id == request.StructureId, ct);

            if (structure is null || structure.Type != StructureType.Chair)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Structure with ID {request.StructureId} not found or is not chair.");
            }

            // Map Command to Entity
            var entity = new EducationProgram
            {
                Code = request.Code,
                Name = request.Name,
                StructureId = request.StructureId,
                ParentId = request.ParentId
            };

            // Persist
            _context.EducationPrograms.Add(entity);
            await _context.SaveChangesAsync(ct);

            return Result.Success(entity.Id, "Program created successfully");
        }
    }
}
