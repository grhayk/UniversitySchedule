using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.GroupSubjectsWithLecturer.RemoveGroupSubjectWithLecturer
{
    internal class RemoveGroupSubjectWithLecturerHandler : IRequestHandler<RemoveGroupSubjectWithLecturerCommand, Result>
    {
        private readonly IDbContext _context;

        public RemoveGroupSubjectWithLecturerHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(RemoveGroupSubjectWithLecturerCommand request, CancellationToken ct)
        {
            var item = await _context.GroupSubjectsWithLecturer
                .FirstOrDefaultAsync(g => g.Id == request.Id, ct);

            if (item is null)
            {
                return Result.Failure(ErrorType.NotFound, $"GroupSubjectWithLecturer with ID {request.Id} not found.");
            }

            _context.GroupSubjectsWithLecturer.Remove(item);
            await _context.SaveChangesAsync(ct);

            return Result.Success("Lecturer-Subject removed from group successfully");
        }
    }
}
