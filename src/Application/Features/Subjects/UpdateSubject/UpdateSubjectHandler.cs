using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Subjects.UpdateSubject
{
    internal class UpdateSubjectHandler : IRequestHandler<UpdateSubjectCommand, Result>
    {
        private readonly IDbContext _context;

        public UpdateSubjectHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateSubjectCommand request, CancellationToken ct)
        {
            // Find subject with configs
            var subject = await _context.Subjects
                .Include(s => s.SubjectConfigs)
                .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

            if (subject is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Subject with ID {request.Id} not found.");
            }

            // Validate structure exists and is of type Chair
            var structure = await _context.Structures.FirstOrDefaultAsync(s => s.Id == request.StructureId, ct);

            if (structure is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Structure with ID {request.StructureId} not found.");
            }

            if (structure.Type != StructureType.Chair)
            {
                return Result.Failure(ErrorType.Validation, $"Subjects can only be assigned to Chair structures. Structure {request.StructureId} is of type {structure.Type}.");
            }

            // Validate SemesterFrom exists
            var semesterFrom = await _context.Semesters.AnyAsync(s => s.Id == request.SemesterIdFrom, ct);
            if (!semesterFrom)
            {
                return Result.Failure(ErrorType.NotFound, $"Semester with ID {request.SemesterIdFrom} not found.");
            }

            // Validate SemesterTo exists
            var semesterTo = await _context.Semesters.AnyAsync(s => s.Id == request.SemesterIdTo, ct);
            if (!semesterTo)
            {
                return Result.Failure(ErrorType.NotFound, $"Semester with ID {request.SemesterIdTo} not found.");
            }

            // Check if another subject has the same code
            var existingSubject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Code == request.Code && s.Id != request.Id, ct);
            if (existingSubject != null)
            {
                return Result.Failure(ErrorType.Conflict, $"Subject with code '{request.Code}' already exists.");
            }

            // Update subject properties
            subject.Code = request.Code;
            subject.Name = request.Name;
            subject.SemesterIdFrom = request.SemesterIdFrom;
            subject.SemesterIdTo = request.SemesterIdTo;
            subject.StructureId = request.StructureId;

            // Remove existing configs and add new ones
            _context.SubjectConfigs.RemoveRange(subject.SubjectConfigs);

            foreach (var configDto in request.Configs)
            {
                subject.SubjectConfigs.Add(new SubjectConfig
                {
                    LessonType = configDto.LessonType,
                    Hours = configDto.Hours
                });
            }

            await _context.SaveChangesAsync(ct);

            return Result.Success("Subject updated successfully");
        }
    }
}
