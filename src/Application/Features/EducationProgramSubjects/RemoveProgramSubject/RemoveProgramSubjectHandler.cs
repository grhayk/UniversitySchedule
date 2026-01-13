using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EducationProgramSubjects.RemoveProgramSubject
{
    internal class RemoveProgramSubjectHandler : IRequestHandler<RemoveProgramSubjectCommand, Result>
    {
        private readonly IDbContext _context;

        public RemoveProgramSubjectHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(RemoveProgramSubjectCommand request, CancellationToken ct)
        {
            var programSubject = await _context.EducationProgramSubjects
                .FirstOrDefaultAsync(eps => eps.Id == request.Id, ct);

            if (programSubject is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Education program subject with ID {request.Id} not found.");
            }

            _context.EducationProgramSubjects.Remove(programSubject);
            await _context.SaveChangesAsync(ct);

            return Result.Success("Subject removed from program successfully");
        }
    }
}
