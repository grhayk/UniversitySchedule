using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.LecturerSubjects.AssignSubjectToLecturer
{
    internal class AssignSubjectToLecturerHandler : IRequestHandler<AssignSubjectToLecturerCommand, Result<int>>
    {
        private readonly IDbContext _context;

        public AssignSubjectToLecturerHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(AssignSubjectToLecturerCommand request, CancellationToken ct)
        {
            // Validate lecturer exists
            var lecturerExists = await _context.Lecturers.AnyAsync(l => l.Id == request.LecturerId, ct);
            if (!lecturerExists)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Lecturer with ID {request.LecturerId} not found.");
            }

            // Validate subject exists
            var subjectExists = await _context.Subjects.AnyAsync(s => s.Id == request.SubjectId, ct);
            if (!subjectExists)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Subject with ID {request.SubjectId} not found.");
            }

            // Check for duplicate
            var duplicateExists = await _context.LecturerSubjects
                .AnyAsync(ls => ls.LecturerId == request.LecturerId && ls.SubjectId == request.SubjectId, ct);

            if (duplicateExists)
            {
                return Result.Failure<int>(ErrorType.Conflict,
                    $"Lecturer {request.LecturerId} is already assigned to subject {request.SubjectId}.");
            }

            var lecturerSubject = new LecturerSubject
            {
                LecturerId = request.LecturerId,
                SubjectId = request.SubjectId
            };

            _context.LecturerSubjects.Add(lecturerSubject);
            await _context.SaveChangesAsync(ct);

            return Result.Success(lecturerSubject.Id, "Subject assigned to lecturer successfully");
        }
    }
}
