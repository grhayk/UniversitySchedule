using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EducationProgramSubjects.AssignSubjectToProgram
{
    internal class AssignSubjectToProgramHandler : IRequestHandler<AssignSubjectToProgramCommand, Result<int>>
    {
        private readonly IDbContext _context;

        public AssignSubjectToProgramHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(AssignSubjectToProgramCommand request, CancellationToken ct)
        {
            // Validate education program exists
            var programExists = await _context.EducationPrograms.AnyAsync(p => p.Id == request.EducationProgramId, ct);
            if (!programExists)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Education program with ID {request.EducationProgramId} not found.");
            }

            // Validate subject exists
            var subjectExists = await _context.Subjects.AnyAsync(s => s.Id == request.SubjectId, ct);
            if (!subjectExists)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Subject with ID {request.SubjectId} not found.");
            }

            // Validate semester exists
            var semesterExists = await _context.Semesters.AnyAsync(s => s.Id == request.SemesterId, ct);
            if (!semesterExists)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Semester with ID {request.SemesterId} not found.");
            }

            // Check for duplicate assignment (same program + subject + semester)
            var duplicateExists = await _context.EducationProgramSubjects
                .AnyAsync(eps => eps.EducationProgramId == request.EducationProgramId
                              && eps.SubjectId == request.SubjectId
                              && eps.SemesterId == request.SemesterId, ct);

            if (duplicateExists)
            {
                return Result.Failure<int>(ErrorType.Conflict,
                    $"Subject {request.SubjectId} is already assigned to program {request.EducationProgramId} for semester {request.SemesterId}.");
            }

            var programSubject = new EducationProgramSubject
            {
                EducationProgramId = request.EducationProgramId,
                SubjectId = request.SubjectId,
                SemesterId = request.SemesterId,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            };

            _context.EducationProgramSubjects.Add(programSubject);
            await _context.SaveChangesAsync(ct);

            return Result.Success(programSubject.Id, "Subject assigned to program successfully");
        }
    }
}
