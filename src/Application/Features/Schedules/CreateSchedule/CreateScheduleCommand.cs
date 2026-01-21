using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Schedules.CreateSchedule
{
    public record CreateScheduleCommand : IRequest<Result<int>>
    {
        public int SubjectId { get; init; }
        public int LecturerId { get; init; }
        public LessonType LessonType { get; init; }
        public int ClassroomId { get; init; }
        public int TimeTableId { get; init; }
        public WeekType WeekType { get; init; }
        public DateTime ScheduleDate { get; init; }
        public int SemesterId { get; init; }
        public int? ScheduleParentId { get; init; }
        public List<int> GroupIds { get; init; } = new();
    }

    public class CreateScheduleValidator : AbstractValidator<CreateScheduleCommand>
    {
        public CreateScheduleValidator()
        {
            RuleFor(x => x.SubjectId).GreaterThan(0);
            RuleFor(x => x.LecturerId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum();
            RuleFor(x => x.ClassroomId).GreaterThan(0);
            RuleFor(x => x.TimeTableId).GreaterThan(0);
            RuleFor(x => x.WeekType).IsInEnum();
            RuleFor(x => x.ScheduleDate).NotEmpty();
            RuleFor(x => x.SemesterId).GreaterThan(0);
            RuleFor(x => x.GroupIds).NotEmpty().WithMessage("At least one group is required");
            RuleForEach(x => x.GroupIds).GreaterThan(0);
        }
    }
}
