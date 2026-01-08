using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Classrooms.DeleteClassroom
{
    internal class DeleteClassroomHandler : IRequestHandler<DeleteClassroomCommand, Result>
    {
        private readonly IDbContext _context;

        public DeleteClassroomHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeleteClassroomCommand request, CancellationToken ct)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.Characteristics)
                .FirstOrDefaultAsync(c => c.Id == request.Id, ct);

            if (classroom is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Classroom with ID {request.Id} not found.");
            }

            // Remove characteristics first (if exists)
            if (classroom.Characteristics != null)
            {
                _context.ClassroomCharacteristics.Remove(classroom.Characteristics);
            }

            // Remove classroom
            _context.Classrooms.Remove(classroom);
            await _context.SaveChangesAsync(ct);

            return Result.Success("Classroom deleted successfully");
        }
    }
}
