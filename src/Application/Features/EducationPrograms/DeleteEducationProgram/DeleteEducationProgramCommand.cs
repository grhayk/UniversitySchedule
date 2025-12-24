using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.EducationPrograms.DeleteEducationProgram
{
    public record DeleteEducationProgramCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class DeleteEducationProgramValidator : AbstractValidator<DeleteEducationProgramCommand>
    {
        public DeleteEducationProgramValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
