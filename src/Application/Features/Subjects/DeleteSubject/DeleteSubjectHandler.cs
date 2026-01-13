using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Subjects.DeleteSubject
{
    internal class DeleteSubjectHandler : IRequestHandler<DeleteSubjectCommand, Result>
    {
        private readonly IDbContext _context;

        public DeleteSubjectHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeleteSubjectCommand request, CancellationToken ct)
        {
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

            if (subject is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Subject with ID {request.Id} not found.");
            }

            // SubjectConfigs will be deleted automatically due to Cascade delete behavior
            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync(ct);

            return Result.Success("Subject deleted successfully");
        }
    }
}
