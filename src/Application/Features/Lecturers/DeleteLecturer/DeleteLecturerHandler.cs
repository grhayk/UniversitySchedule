using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Lecturers.DeleteLecturer
{
    internal class DeleteLecturerHandler : IRequestHandler<DeleteLecturerCommand, Result>
    {
        private readonly IDbContext _context;

        public DeleteLecturerHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeleteLecturerCommand request, CancellationToken ct)
        {
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.Id == request.Id, ct);

            if (lecturer is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Lecturer with ID {request.Id} not found.");
            }

            _context.Lecturers.Remove(lecturer);
            await _context.SaveChangesAsync(ct);

            return Result.Success("Lecturer deleted successfully");
        }
    }
}
