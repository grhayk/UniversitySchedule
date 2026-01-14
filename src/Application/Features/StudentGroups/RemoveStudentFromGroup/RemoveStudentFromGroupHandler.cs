using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.StudentGroups.RemoveStudentFromGroup
{
    internal class RemoveStudentFromGroupHandler : IRequestHandler<RemoveStudentFromGroupCommand, Result>
    {
        private readonly IDbContext _context;

        public RemoveStudentFromGroupHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(RemoveStudentFromGroupCommand request, CancellationToken ct)
        {
            var studentGroup = await _context.StudentGroups
                .Include(sg => sg.Group)
                .Include(sg => sg.Student)
                .FirstOrDefaultAsync(sg => sg.Id == request.Id, ct);

            if (studentGroup is null)
            {
                return Result.Failure(ErrorType.NotFound, $"StudentGroup with ID {request.Id} not found.");
            }

            // Cannot remove from parent group - that's the student's main group
            if (studentGroup.Group.ParentId == null && studentGroup.GroupId == studentGroup.Student.GroupId)
            {
                return Result.Failure(ErrorType.Validation,
                    "Cannot remove student from their main (parent) group. Use Update Student to change the main group.");
            }

            _context.StudentGroups.Remove(studentGroup);
            await _context.SaveChangesAsync(ct);

            return Result.Success("Student removed from group successfully");
        }
    }
}
