using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Groups.GetGroup
{
    public record GetGroupQuery : IRequest<Result<GroupDto>>
    {
        public int Id { get; init; }
    }

    public class GetGroupValidator : AbstractValidator<GetGroupQuery>
    {
        public GetGroupValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
