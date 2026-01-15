using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.SubjectClassrooms.RemoveSubjectClassroom
{
    internal class RemoveSubjectClassroomHandler : IRequestHandler<RemoveSubjectClassroomCommand, Result>
    {
        private readonly IDbContext _context;

        public RemoveSubjectClassroomHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(RemoveSubjectClassroomCommand request, CancellationToken ct)
        {
            var subjectClassroom = await _context.SubjectClassrooms
                .FirstOrDefaultAsync(sc => sc.Id == request.Id, ct);

            if (subjectClassroom is null)
            {
                return Result.Failure(ErrorType.NotFound, $"SubjectClassroom with ID {request.Id} not found.");
            }

            _context.SubjectClassrooms.Remove(subjectClassroom);
            await _context.SaveChangesAsync(ct);

            return Result.Success("Classroom removed from subject successfully");
        }
    }
}
