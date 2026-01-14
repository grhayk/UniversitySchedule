using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.StudentGroups.AssignStudentToGroup
{
    internal class AssignStudentToGroupHandler : IRequestHandler<AssignStudentToGroupCommand, Result<int>>
    {
        private readonly IDbContext _context;

        public AssignStudentToGroupHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(AssignStudentToGroupCommand request, CancellationToken ct)
        {
            // Get student with their parent group
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);

            if (student is null)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Student with ID {request.StudentId} not found.");
            }

            // Get group with semester
            var group = await _context.Groups
                .Include(g => g.Semester)
                .FirstOrDefaultAsync(g => g.Id == request.GroupId, ct);

            if (group is null)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Group with ID {request.GroupId} not found.");
            }

            // Validate student belongs to this group's parent (or this group itself if it's a parent group)
            var parentGroupId = group.ParentId ?? group.Id;
            if (student.GroupId != parentGroupId)
            {
                return Result.Failure<int>(ErrorType.Validation,
                    $"Student does not belong to the parent group (ID: {parentGroupId}). Student's group: {student.GroupId}");
            }

            // Check for duplicate assignment
            var duplicateExists = await _context.StudentGroups
                .AnyAsync(sg => sg.StudentId == request.StudentId && sg.GroupId == request.GroupId, ct);

            if (duplicateExists)
            {
                return Result.Failure<int>(ErrorType.Conflict,
                    $"Student {request.StudentId} is already assigned to group {request.GroupId}.");
            }

            var studentGroup = new StudentGroup
            {
                StudentId = request.StudentId,
                GroupId = request.GroupId,
                SemesterId = group.SemesterId
            };

            _context.StudentGroups.Add(studentGroup);
            await _context.SaveChangesAsync(ct);

            return Result.Success(studentGroup.Id, "Student assigned to group successfully");
        }
    }
}
