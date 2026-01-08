using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Classrooms.CreateClassroom
{
    internal class CreateClassroomHandler : IRequestHandler<CreateClassroomCommand, Result<int>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public CreateClassroomHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<int>> Handle(CreateClassroomCommand request, CancellationToken ct)
        {
            // Validate structure exists and is of type Chair
            var structure = await _context.Structures.FirstOrDefaultAsync(s => s.Id == request.StructureId, ct);

            if (structure is null)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Structure with ID {request.StructureId} not found.");
            }

            if (structure.Type != StructureType.Chair)
            {
                return Result.Failure<int>(ErrorType.Validation, $"Classrooms can only be assigned to Chair structures. Structure {request.StructureId} is of type {structure.Type}.");
            }

            // Check if classroom name already exists (unique constraint)
            var existingClassroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Name == request.Name, ct);
            if (existingClassroom != null)
            {
                return Result.Failure<int>(ErrorType.Conflict, $"Classroom with name '{request.Name}' already exists.");
            }

            // Map command to entities
            var classroom = _mapper.Map<Classroom>(request);
            var characteristics = _mapper.Map<ClassroomCharacteristics>(request.Characteristics);

            // Set up the relationship
            classroom.Characteristics = characteristics;

            // Persist
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync(ct);

            return Result.Success(classroom.Id, "Classroom created successfully");
        }
    }
}
