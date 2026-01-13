using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EducationProgramSubjects.UpdateProgramSubject
{
    internal class UpdateProgramSubjectHandler : IRequestHandler<UpdateProgramSubjectCommand, Result>
    {
        private readonly IDbContext _context;

        public UpdateProgramSubjectHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateProgramSubjectCommand request, CancellationToken ct)
        {
            var programSubject = await _context.EducationProgramSubjects
                .FirstOrDefaultAsync(eps => eps.Id == request.Id, ct);

            if (programSubject is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Education program subject with ID {request.Id} not found.");
            }

            // Validate semester exists
            var semesterExists = await _context.Semesters.AnyAsync(s => s.Id == request.SemesterId, ct);
            if (!semesterExists)
            {
                return Result.Failure(ErrorType.NotFound, $"Semester with ID {request.SemesterId} not found.");
            }

            // Check for duplicate if semester is changing
            if (programSubject.SemesterId != request.SemesterId)
            {
                var duplicateExists = await _context.EducationProgramSubjects
                    .AnyAsync(eps => eps.EducationProgramId == programSubject.EducationProgramId
                                  && eps.SubjectId == programSubject.SubjectId
                                  && eps.SemesterId == request.SemesterId
                                  && eps.Id != request.Id, ct);

                if (duplicateExists)
                {
                    return Result.Failure(ErrorType.Conflict,
                        $"This subject is already assigned to the program for semester {request.SemesterId}.");
                }
            }

            programSubject.SemesterId = request.SemesterId;
            programSubject.FromDate = request.FromDate;
            programSubject.ToDate = request.ToDate;

            await _context.SaveChangesAsync(ct);

            return Result.Success("Program subject updated successfully");
        }
    }
}
