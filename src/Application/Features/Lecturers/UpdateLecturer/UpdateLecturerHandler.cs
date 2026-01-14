using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Lecturers.UpdateLecturer
{
    internal class UpdateLecturerHandler : IRequestHandler<UpdateLecturerCommand, Result>
    {
        private readonly IDbContext _context;

        public UpdateLecturerHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateLecturerCommand request, CancellationToken ct)
        {
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.Id == request.Id, ct);

            if (lecturer is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Lecturer with ID {request.Id} not found.");
            }

            // Validate structure exists
            var structureExists = await _context.Structures.AnyAsync(s => s.Id == request.StructureId, ct);

            if (!structureExists)
            {
                return Result.Failure(ErrorType.NotFound, $"Structure with ID {request.StructureId} not found.");
            }

            lecturer.FirstName = request.FirstName;
            lecturer.LastName = request.LastName;
            lecturer.BirthDate = request.BirthDate;
            lecturer.StructureId = request.StructureId;

            await _context.SaveChangesAsync(ct);

            return Result.Success("Lecturer updated successfully");
        }
    }
}
