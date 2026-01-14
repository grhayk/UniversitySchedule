using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Students.DeleteStudent
{
    internal class DeleteStudentHandler : IRequestHandler<DeleteStudentCommand, Result>
    {
        private readonly IDbContext _context;

        public DeleteStudentHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeleteStudentCommand request, CancellationToken ct)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == request.Id, ct);

            if (student is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Student with ID {request.Id} not found.");
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync(ct);

            return Result.Success("Student deleted successfully");
        }
    }
}
