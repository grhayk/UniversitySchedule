using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Schedules.GetScheduleById
{
    public record GetScheduleByIdQuery : IRequest<Result<ScheduleDetailDto>>
    {
        public int Id { get; init; }
    }

    public class GetScheduleByIdValidator : AbstractValidator<GetScheduleByIdQuery>
    {
        public GetScheduleByIdValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
