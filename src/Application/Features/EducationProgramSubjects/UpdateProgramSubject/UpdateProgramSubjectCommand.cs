using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.EducationProgramSubjects.UpdateProgramSubject
{
    public record UpdateProgramSubjectCommand : IRequest<Result>
    {
        public int Id { get; init; }
        public int SemesterId { get; init; }
        public DateTime FromDate { get; init; }
        public DateTime? ToDate { get; init; }
    }

    public class UpdateProgramSubjectValidator : AbstractValidator<UpdateProgramSubjectCommand>
    {
        public UpdateProgramSubjectValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.SemesterId).GreaterThan(0);
            RuleFor(x => x.FromDate).NotEmpty();
            RuleFor(x => x.ToDate)
                .GreaterThan(x => x.FromDate)
                .When(x => x.ToDate.HasValue)
                .WithMessage("ToDate must be after FromDate");
        }
    }
}
