using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Groups.CreateGroup
{
    internal class CreateGroupHandler : IRequestHandler<CreateGroupCommand, Result<int>>
    {
        private readonly IDbContext _context;

        public CreateGroupHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(CreateGroupCommand request, CancellationToken ct)
        {
            // Validate education program exists
            var programExists = await _context.EducationPrograms.AnyAsync(p => p.Id == request.EducationProgramId, ct);
            if (!programExists)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Education program with ID {request.EducationProgramId} not found.");
            }

            // Validate semester exists
            var semesterExists = await _context.Semesters.AnyAsync(s => s.Id == request.SemesterId, ct);
            if (!semesterExists)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Semester with ID {request.SemesterId} not found.");
            }

            // Validate parent group exists if specified
            if (request.ParentId.HasValue)
            {
                var parentGroup = await _context.Groups.FirstOrDefaultAsync(g => g.Id == request.ParentId, ct);
                if (parentGroup is null)
                {
                    return Result.Failure<int>(ErrorType.NotFound, $"Parent group with ID {request.ParentId} not found.");
                }

                // Parent should be a main group (Lecture type)
                if (parentGroup.LessonType != LessonType.Lecture)
                {
                    return Result.Failure<int>(ErrorType.Validation, "Parent group must be a main group (LessonType = Lecture).");
                }
            }

            // Validate branched from group exists if specified
            if (request.BranchedFromGroupId.HasValue)
            {
                var branchedFromExists = await _context.Groups.AnyAsync(g => g.Id == request.BranchedFromGroupId, ct);
                if (!branchedFromExists)
                {
                    return Result.Failure<int>(ErrorType.NotFound, $"Branched from group with ID {request.BranchedFromGroupId} not found.");
                }
            }

            // Check for duplicate (same program, semester, lesson type, index, parent)
            var duplicateExists = await _context.Groups
                .AnyAsync(g => g.EducationProgramId == request.EducationProgramId
                            && g.SemesterId == request.SemesterId
                            && g.LessonType == request.LessonType
                            && g.IndexNumber == request.IndexNumber
                            && g.ParentId == request.ParentId, ct);

            if (duplicateExists)
            {
                return Result.Failure<int>(ErrorType.Conflict,
                    $"Group with same program, semester, lesson type, index, and parent already exists.");
            }

            var group = new Group
            {
                ParentId = request.ParentId,
                EducationProgramId = request.EducationProgramId,
                SemesterId = request.SemesterId,
                LessonType = request.LessonType,
                IsActive = request.IsActive,
                StartDate = request.StartDate,
                IndexNumber = request.IndexNumber,
                BranchedFromGroupId = request.BranchedFromGroupId
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync(ct);

            return Result.Success(group.Id, "Group created successfully");
        }
    }
}
