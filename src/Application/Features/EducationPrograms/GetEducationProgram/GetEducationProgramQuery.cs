using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.EducationPrograms.GetEducationProgram
{
    public record GetEducationProgramQuery : IRequest<Result<EducationProgramDto>>
    {
        public int Id { get; init; }
    }

    public class GetEducationProgramValidator : AbstractValidator<GetEducationProgramQuery>
    {
        public GetEducationProgramValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
