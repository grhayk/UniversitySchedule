using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Groups.UpdateGroup
{
    internal class UpdateGroupHandler : IRequestHandler<UpdateGroupCommand, Result>
    {
        private readonly IDbContext _context;

        public UpdateGroupHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateGroupCommand request, CancellationToken ct)
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == request.Id, ct);

            if (group is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Group with ID {request.Id} not found.");
            }

            // Validate education program exists
            var programExists = await _context.EducationPrograms.AnyAsync(p => p.Id == request.EducationProgramId, ct);
            if (!programExists)
            {
                return Result.Failure(ErrorType.NotFound, $"Education program with ID {request.EducationProgramId} not found.");
            }

            // Validate semester exists
            var semesterExists = await _context.Semesters.AnyAsync(s => s.Id == request.SemesterId, ct);
            if (!semesterExists)
            {
                return Result.Failure(ErrorType.NotFound, $"Semester with ID {request.SemesterId} not found.");
            }

            // Validate parent group exists if specified
            if (request.ParentId.HasValue)
            {
                // Can't be parent of itself
                if (request.ParentId == request.Id)
                {
                    return Result.Failure(ErrorType.Validation, "Group cannot be its own parent.");
                }

                var parentGroup = await _context.Groups.FirstOrDefaultAsync(g => g.Id == request.ParentId, ct);
                if (parentGroup is null)
                {
                    return Result.Failure(ErrorType.NotFound, $"Parent group with ID {request.ParentId} not found.");
                }

                // Parent should be a main group (Lecture type)
                if (parentGroup.LessonType != LessonType.Lecture)
                {
                    return Result.Failure(ErrorType.Validation, "Parent group must be a main group (LessonType = Lecture).");
                }
            }

            // Validate branched from group exists if specified
            if (request.BranchedFromGroupId.HasValue)
            {
                var branchedFromExists = await _context.Groups.AnyAsync(g => g.Id == request.BranchedFromGroupId, ct);
                if (!branchedFromExists)
                {
                    return Result.Failure(ErrorType.NotFound, $"Branched from group with ID {request.BranchedFromGroupId} not found.");
                }
            }

            // Check for duplicate (same program, semester, lesson type, index, parent - excluding current)
            var duplicateExists = await _context.Groups
                .AnyAsync(g => g.EducationProgramId == request.EducationProgramId
                            && g.SemesterId == request.SemesterId
                            && g.LessonType == request.LessonType
                            && g.IndexNumber == request.IndexNumber
                            && g.ParentId == request.ParentId
                            && g.Id != request.Id, ct);

            if (duplicateExists)
            {
                return Result.Failure(ErrorType.Conflict,
                    $"Group with same program, semester, lesson type, index, and parent already exists.");
            }

            group.ParentId = request.ParentId;
            group.EducationProgramId = request.EducationProgramId;
            group.SemesterId = request.SemesterId;
            group.LessonType = request.LessonType;
            group.IsActive = request.IsActive;
            group.StartDate = request.StartDate;
            group.IndexNumber = request.IndexNumber;
            group.BranchedFromGroupId = request.BranchedFromGroupId;

            await _context.SaveChangesAsync(ct);

            return Result.Success("Group updated successfully");
        }
    }
}
