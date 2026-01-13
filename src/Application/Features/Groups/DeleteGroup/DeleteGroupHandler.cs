using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Groups.DeleteGroup
{
    internal class DeleteGroupHandler : IRequestHandler<DeleteGroupCommand, Result>
    {
        private readonly IDbContext _context;

        public DeleteGroupHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeleteGroupCommand request, CancellationToken ct)
        {
            var group = await _context.Groups
                .Include(g => g.Children)
                .FirstOrDefaultAsync(g => g.Id == request.Id, ct);

            if (group is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Group with ID {request.Id} not found.");
            }

            // Check if group has children (subgroups)
            if (group.Children.Any())
            {
                return Result.Failure(ErrorType.Validation,
                    $"Cannot delete group with ID {request.Id} because it has {group.Children.Count} subgroup(s). Delete subgroups first.");
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync(ct);

            return Result.Success("Group deleted successfully");
        }
    }
}
