using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.EducationProgramSubjects.AssignSubjectToProgram
{
    public record AssignSubjectToProgramCommand : IRequest<Result<int>>
    {
        public int EducationProgramId { get; init; }
        public int SubjectId { get; init; }
        public int SemesterId { get; init; }
        public DateTime FromDate { get; init; }
        public DateTime? ToDate { get; init; }
    }

    public class AssignSubjectToProgramValidator : AbstractValidator<AssignSubjectToProgramCommand>
    {
        public AssignSubjectToProgramValidator()
        {
            RuleFor(x => x.EducationProgramId).GreaterThan(0);
            RuleFor(x => x.SubjectId).GreaterThan(0);
            RuleFor(x => x.SemesterId).GreaterThan(0);
            RuleFor(x => x.FromDate).NotEmpty();
            RuleFor(x => x.ToDate)
                .GreaterThan(x => x.FromDate)
                .When(x => x.ToDate.HasValue)
                .WithMessage("ToDate must be after FromDate");
        }
    }
}
