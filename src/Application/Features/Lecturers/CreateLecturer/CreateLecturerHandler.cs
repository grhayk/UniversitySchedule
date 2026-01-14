using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Lecturers.CreateLecturer
{
    internal class CreateLecturerHandler : IRequestHandler<CreateLecturerCommand, Result<int>>
    {
        private readonly IDbContext _context;

        public CreateLecturerHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(CreateLecturerCommand request, CancellationToken ct)
        {
            // Validate structure exists
            var structureExists = await _context.Structures.AnyAsync(s => s.Id == request.StructureId, ct);

            if (!structureExists)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Structure with ID {request.StructureId} not found.");
            }

            var lecturer = new Lecturer
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                StructureId = request.StructureId
            };

            _context.Lecturers.Add(lecturer);
            await _context.SaveChangesAsync(ct);

            return Result.Success(lecturer.Id, "Lecturer created successfully");
        }
    }
}
