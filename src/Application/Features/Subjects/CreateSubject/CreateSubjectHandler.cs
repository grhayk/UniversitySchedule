using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Subjects.CreateSubject
{
    internal class CreateSubjectHandler : IRequestHandler<CreateSubjectCommand, Result<int>>
    {
        private readonly IDbContext _context;

        public CreateSubjectHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(CreateSubjectCommand request, CancellationToken ct)
        {
            // Validate structure exists and is of type Chair
            var structure = await _context.Structures.FirstOrDefaultAsync(s => s.Id == request.StructureId, ct);

            if (structure is null)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Structure with ID {request.StructureId} not found.");
            }

            if (structure.Type != StructureType.Chair)
            {
                return Result.Failure<int>(ErrorType.Validation, $"Subjects can only be assigned to Chair structures. Structure {request.StructureId} is of type {structure.Type}.");
            }

            // Validate SemesterFrom exists
            var semesterFrom = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == request.SemesterIdFrom, ct);
            if (semesterFrom is null)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Semester with ID {request.SemesterIdFrom} not found.");
            }

            // Validate SemesterTo exists
            var semesterTo = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == request.SemesterIdTo, ct);
            if (semesterTo is null)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Semester with ID {request.SemesterIdTo} not found.");
            }

            // Check if subject code already exists
            var existingSubject = await _context.Subjects.FirstOrDefaultAsync(s => s.Code == request.Code, ct);
            if (existingSubject != null)
            {
                return Result.Failure<int>(ErrorType.Conflict, $"Subject with code '{request.Code}' already exists.");
            }

            // Create subject with configs
            var subject = new Subject
            {
                Code = request.Code,
                Name = request.Name,
                SemesterIdFrom = request.SemesterIdFrom,
                SemesterIdTo = request.SemesterIdTo,
                StructureId = request.StructureId
            };

            // Add configs
            foreach (var configDto in request.Configs)
            {
                subject.SubjectConfigs.Add(new SubjectConfig
                {
                    LessonType = configDto.LessonType,
                    Hours = configDto.Hours
                });
            }

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync(ct);

            return Result.Success(subject.Id, "Subject created successfully");
        }
    }
}
