using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Schedules.GetSchedulesBySemester
{
    public record GetSchedulesBySemesterQuery : IRequest<Result<List<ScheduleListDto>>>
    {
        public int SemesterId { get; init; }
        public DateTime? DateFrom { get; init; }
        public DateTime? DateTo { get; init; }
        public LessonType? LessonType { get; init; }
        public WeekType? WeekType { get; init; }
        public int? LecturerId { get; init; }
        public int? SubjectId { get; init; }
    }

    public class GetSchedulesBySemesterValidator : AbstractValidator<GetSchedulesBySemesterQuery>
    {
        public GetSchedulesBySemesterValidator()
        {
            RuleFor(x => x.SemesterId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum().When(x => x.LessonType.HasValue);
            RuleFor(x => x.WeekType).IsInEnum().When(x => x.WeekType.HasValue);
        }
    }
}
