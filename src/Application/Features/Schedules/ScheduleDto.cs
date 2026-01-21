using Domain.Enums;

namespace Application.Features.Schedules
{
    public record ScheduleDto
    {
        public int Id { get; init; }
        public int SubjectId { get; init; }
        public int TimeTableId { get; init; }
        public LessonType LessonType { get; init; }
        public WeekType WeekType { get; init; }
        public int ClassroomId { get; init; }
        public int LecturerId { get; init; }
        public int? ScheduleParentId { get; init; }
        public int SemesterId { get; init; }
        public DateTime ScheduleDate { get; init; }
        public List<int> GroupIds { get; init; } = new();
    }

    public record ScheduleDetailDto
    {
        public int Id { get; init; }
        public int SubjectId { get; init; }
        public string SubjectName { get; init; } = null!;
        public int TimeTableId { get; init; }
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
        public LessonType LessonType { get; init; }
        public WeekType WeekType { get; init; }
        public int ClassroomId { get; init; }
        public string ClassroomName { get; init; } = null!;
        public int LecturerId { get; init; }
        public string LecturerName { get; init; } = null!;
        public int? ScheduleParentId { get; init; }
        public int SemesterId { get; init; }
        public DateTime ScheduleDate { get; init; }
        public List<ScheduleGroupDto> Groups { get; init; } = new();
    }

    public record ScheduleGroupDto
    {
        public int GroupId { get; init; }
        public int IndexNumber { get; init; }
        public LessonType GroupLessonType { get; init; }
    }

    public record ScheduleListDto
    {
        public int Id { get; init; }
        public string SubjectName { get; init; } = null!;
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
        public LessonType LessonType { get; init; }
        public WeekType WeekType { get; init; }
        public string ClassroomName { get; init; } = null!;
        public string LecturerName { get; init; } = null!;
        public DateTime ScheduleDate { get; init; }
        public int GroupCount { get; init; }
    }
}
