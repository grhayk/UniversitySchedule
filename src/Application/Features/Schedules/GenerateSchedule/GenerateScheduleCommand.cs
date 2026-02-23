using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Schedules.GenerateSchedule
{
    public record GenerateScheduleCommand : IRequest<Result<GenerateScheduleResult>>
    {
        /// <summary>
        /// First Monday of the semester, used for setting ScheduleDate on generated entries.
        /// </summary>
        public DateTime StartDate { get; init; }
    }

    public class GenerateScheduleValidator : AbstractValidator<GenerateScheduleCommand>
    {
        public GenerateScheduleValidator()
        {
            RuleFor(x => x.StartDate).NotEmpty();
        }
    }
}
