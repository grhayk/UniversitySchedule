using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.LecturerSubjects.RemoveLecturerSubject
{
    internal class RemoveLecturerSubjectHandler : IRequestHandler<RemoveLecturerSubjectCommand, Result>
    {
        private readonly IDbContext _context;

        public RemoveLecturerSubjectHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(RemoveLecturerSubjectCommand request, CancellationToken ct)
        {
            var lecturerSubject = await _context.LecturerSubjects
                .FirstOrDefaultAsync(ls => ls.Id == request.Id, ct);

            if (lecturerSubject is null)
            {
                return Result.Failure(ErrorType.NotFound, $"LecturerSubject with ID {request.Id} not found.");
            }

            _context.LecturerSubjects.Remove(lecturerSubject);
            await _context.SaveChangesAsync(ct);

            return Result.Success("Subject removed from lecturer successfully");
        }
    }
}
