using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.SubjectClassrooms.AssignClassroomToSubject
{
    internal class AssignClassroomToSubjectHandler : IRequestHandler<AssignClassroomToSubjectCommand, Result<int>>
    {
        private readonly IDbContext _context;

        public AssignClassroomToSubjectHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(AssignClassroomToSubjectCommand request, CancellationToken ct)
        {
            // Validate subject exists and has the specified lesson type in its configs
            var subject = await _context.Subjects
                .Include(s => s.SubjectConfigs)
                .FirstOrDefaultAsync(s => s.Id == request.SubjectId, ct);

            if (subject is null)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Subject with ID {request.SubjectId} not found.");
            }

            // Check subject has this lesson type configured
            var hasLessonType = subject.SubjectConfigs.Any(c => c.LessonType == request.LessonType);
            if (!hasLessonType)
            {
                return Result.Failure<int>(ErrorType.Validation,
                    $"Subject does not have {request.LessonType} configured in its SubjectConfigs.");
            }

            // Validate classroom exists
            var classroomExists = await _context.Classrooms.AnyAsync(c => c.Id == request.ClassroomId, ct);
            if (!classroomExists)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Classroom with ID {request.ClassroomId} not found.");
            }

            // Check for duplicate
            var duplicateExists = await _context.SubjectClassrooms
                .AnyAsync(sc => sc.SubjectId == request.SubjectId
                             && sc.LessonType == request.LessonType
                             && sc.ClassroomId == request.ClassroomId, ct);

            if (duplicateExists)
            {
                return Result.Failure<int>(ErrorType.Conflict,
                    $"Classroom {request.ClassroomId} is already assigned to subject {request.SubjectId} for {request.LessonType}.");
            }

            var subjectClassroom = new SubjectClassroom
            {
                SubjectId = request.SubjectId,
                LessonType = request.LessonType,
                ClassroomId = request.ClassroomId
            };

            _context.SubjectClassrooms.Add(subjectClassroom);
            await _context.SaveChangesAsync(ct);

            return Result.Success(subjectClassroom.Id, "Classroom assigned to subject successfully");
        }
    }
}
