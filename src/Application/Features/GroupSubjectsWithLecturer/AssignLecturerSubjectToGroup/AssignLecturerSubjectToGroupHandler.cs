using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.GroupSubjectsWithLecturer.AssignLecturerSubjectToGroup
{
    internal class AssignLecturerSubjectToGroupHandler : IRequestHandler<AssignLecturerSubjectToGroupCommand, Result<int>>
    {
        private readonly IDbContext _context;

        public AssignLecturerSubjectToGroupHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(AssignLecturerSubjectToGroupCommand request, CancellationToken ct)
        {
            // Get LecturerSubject with Subject and its configs
            var lecturerSubject = await _context.LecturerSubjects
                .Include(ls => ls.Subject)
                    .ThenInclude(s => s.SubjectConfigs)
                .FirstOrDefaultAsync(ls => ls.Id == request.LecturerSubjectId, ct);

            if (lecturerSubject is null)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"LecturerSubject with ID {request.LecturerSubjectId} not found.");
            }

            // Validate group exists
            var groupExists = await _context.Groups.AnyAsync(g => g.Id == request.GroupId, ct);
            if (!groupExists)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Group with ID {request.GroupId} not found.");
            }

            // Check if Subject has this LessonType configured and get the Hours
            var subjectConfig = lecturerSubject.Subject.SubjectConfigs
                .FirstOrDefault(c => c.LessonType == request.LessonType);

            if (subjectConfig is null)
            {
                return Result.Failure<int>(ErrorType.Validation,
                    $"Subject does not have {request.LessonType} configured in its SubjectConfigs.");
            }

            // Check for duplicate
            var duplicateExists = await _context.GroupSubjectsWithLecturer
                .AnyAsync(g => g.LecturerSubjectId == request.LecturerSubjectId
                            && g.GroupId == request.GroupId
                            && g.LessonType == request.LessonType, ct);

            if (duplicateExists)
            {
                return Result.Failure<int>(ErrorType.Conflict,
                    $"This LecturerSubject is already assigned to group {request.GroupId} for {request.LessonType}.");
            }

            var groupSubjectWithLecturer = new GroupSubjectWithLecturer
            {
                LecturerSubjectId = request.LecturerSubjectId,
                GroupId = request.GroupId,
                LessonType = request.LessonType,
                Hours = subjectConfig.Hours // Hours derived from SubjectConfig
            };

            _context.GroupSubjectsWithLecturer.Add(groupSubjectWithLecturer);
            await _context.SaveChangesAsync(ct);

            return Result.Success(groupSubjectWithLecturer.Id, "Lecturer-Subject assigned to group successfully");
        }
    }
}
