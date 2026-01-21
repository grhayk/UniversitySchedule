using Domain.Enums;
using FluentValidation;

namespace Application.Features.Schedules.BulkUpload
{
    public record CsvScheduleRecord
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
        /// <summary>
        /// Pipe-separated list of GroupIds (e.g., "1|2|3")
        /// </summary>
        public string GroupIds { get; init; } = null!;
    }

    public class CsvScheduleValidator : AbstractValidator<CsvScheduleRecord>
    {
        public CsvScheduleValidator()
        {
            RuleFor(x => x.SubjectId).GreaterThan(0);
            RuleFor(x => x.LecturerId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum();
            RuleFor(x => x.ClassroomId).GreaterThan(0);
            RuleFor(x => x.TimeTableId).GreaterThan(0);
            RuleFor(x => x.WeekType).IsInEnum();
            RuleFor(x => x.ScheduleDate).NotEmpty();
            RuleFor(x => x.SemesterId).GreaterThan(0);
            RuleFor(x => x.GroupIds).NotEmpty().WithMessage("GroupIds is required");
        }
    }
}
