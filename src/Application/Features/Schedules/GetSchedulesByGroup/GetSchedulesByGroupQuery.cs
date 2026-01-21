using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Schedules.GetSchedulesByGroup
{
    public record GetSchedulesByGroupQuery : IRequest<Result<List<ScheduleListDto>>>
    {
        public int GroupId { get; init; }
        public DateTime? DateFrom { get; init; }
        public DateTime? DateTo { get; init; }
        public LessonType? LessonType { get; init; }
        public WeekType? WeekType { get; init; }
    }

    public class GetSchedulesByGroupValidator : AbstractValidator<GetSchedulesByGroupQuery>
    {
        public GetSchedulesByGroupValidator()
        {
            RuleFor(x => x.GroupId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum().When(x => x.LessonType.HasValue);
            RuleFor(x => x.WeekType).IsInEnum().When(x => x.WeekType.HasValue);
        }
    }
}
