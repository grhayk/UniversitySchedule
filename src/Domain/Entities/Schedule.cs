using Domain.Enums;

namespace Domain.Entities
{
    public class Schedule
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int TimeTableId { get; set; }
        public LessonType LessonTypeId { get; set; }
        public WeekType WeekType { get; set; }
        public int ClassroomId { get; set; }
        public int StaffId { get; set; }
        public int? ScheduleParentId { get; set; }
        public int SemesterId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public Subject Subject { get; set; } = null!;
        public TimeTable TimeTable { get; set; } = null!;
        public Classroom Classroom { get; set; } = null!;
        public Staff Staff { get; set; } = null!;
        public Schedule? ScheduleParent { get; set; }
        public ICollection<Schedule> ScheduleExceptions { get; set; } = new List<Schedule>();
        public Semester Semester { get; set; } = null!;
        public ICollection<ScheduleGroup> ScheduleGroups { get; set; } = new List<ScheduleGroup>();
    }
}
