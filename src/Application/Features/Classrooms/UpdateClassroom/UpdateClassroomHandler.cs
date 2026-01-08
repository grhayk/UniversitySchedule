using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Classrooms.UpdateClassroom
{
    internal class UpdateClassroomHandler : IRequestHandler<UpdateClassroomCommand, Result>
    {
        private readonly IDbContext _context;

        public UpdateClassroomHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateClassroomCommand request, CancellationToken ct)
        {
            // Find the classroom with characteristics
            var classroom = await _context.Classrooms
                .Include(c => c.Characteristics)
                .FirstOrDefaultAsync(c => c.Id == request.Id, ct);

            if (classroom is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Classroom with ID {request.Id} not found.");
            }

            // Validate structure exists and is of type Chair
            var structure = await _context.Structures.FirstOrDefaultAsync(s => s.Id == request.StructureId, ct);

            if (structure is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Structure with ID {request.StructureId} not found.");
            }

            if (structure.Type != StructureType.Chair)
            {
                return Result.Failure(ErrorType.Validation, $"Classrooms can only be assigned to Chair structures. Structure {request.StructureId} is of type {structure.Type}.");
            }

            // Check if another classroom has the same name (excluding current)
            var existingClassroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Name == request.Name && c.Id != request.Id, ct);

            if (existingClassroom != null)
            {
                return Result.Failure(ErrorType.Conflict, $"Classroom with name '{request.Name}' already exists.");
            }

            // Update classroom properties
            classroom.Name = request.Name;
            classroom.StructureId = request.StructureId;

            // Update or create characteristics
            if (classroom.Characteristics == null)
            {
                classroom.Characteristics = new Domain.Entities.ClassroomCharacteristics();
            }

            classroom.Characteristics.Type = request.Characteristics.Type;
            classroom.Characteristics.SeatCapacity = request.Characteristics.SeatCapacity;
            classroom.Characteristics.HasComputer = request.Characteristics.HasComputer;
            classroom.Characteristics.ComputerCount = request.Characteristics.ComputerCount;
            classroom.Characteristics.HasProjector = request.Characteristics.HasProjector;
            classroom.Characteristics.RenovationStatus = request.Characteristics.RenovationStatus;
            classroom.Characteristics.BlackboardCondition = request.Characteristics.BlackboardCondition;

            await _context.SaveChangesAsync(ct);

            return Result.Success("Classroom updated successfully");
        }
    }
}
