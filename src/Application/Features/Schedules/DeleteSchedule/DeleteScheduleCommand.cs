using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Schedules.DeleteSchedule
{
    public record DeleteScheduleCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class DeleteScheduleValidator : AbstractValidator<DeleteScheduleCommand>
    {
        public DeleteScheduleValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
