using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.LecturerSubjects.AssignSubjectToLecturer
{
    public record AssignSubjectToLecturerCommand : IRequest<Result<int>>
    {
        public int LecturerId { get; init; }
        public int SubjectId { get; init; }
    }

    public class AssignSubjectToLecturerValidator : AbstractValidator<AssignSubjectToLecturerCommand>
    {
        public AssignSubjectToLecturerValidator()
        {
            RuleFor(x => x.LecturerId).GreaterThan(0);
            RuleFor(x => x.SubjectId).GreaterThan(0);
        }
    }
}
