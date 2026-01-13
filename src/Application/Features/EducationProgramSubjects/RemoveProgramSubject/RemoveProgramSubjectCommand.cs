using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.EducationProgramSubjects.RemoveProgramSubject
{
    public record RemoveProgramSubjectCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class RemoveProgramSubjectValidator : AbstractValidator<RemoveProgramSubjectCommand>
    {
        public RemoveProgramSubjectValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
