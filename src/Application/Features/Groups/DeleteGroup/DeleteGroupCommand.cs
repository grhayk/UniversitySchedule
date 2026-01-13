using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Groups.DeleteGroup
{
    public record DeleteGroupCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class DeleteGroupValidator : AbstractValidator<DeleteGroupCommand>
    {
        public DeleteGroupValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
