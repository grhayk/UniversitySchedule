using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a schedule entry for a lesson.
    /// </summary>
    /// <remarks>
    /// Schedules support recurring patterns through parent-child relationships.
    /// The numerator and denominator week patterns are created as parent schedules,
    /// and subsequent weeks are linked via ScheduleParentId for easy updates and exceptions.
    /// </remarks>
    public class Schedule : BaseEntity
    {
        /// <summary>
        /// Gets or sets the subject ID for this schedule entry.
        /// </summary>
        public int SubjectId { get; set; }

        /// <summary>
        /// Gets or sets the time slot for this lesson.
        /// </summary>
        public int TimeTableId { get; set; }

        /// <summary>
        /// Gets or sets the type of lesson.
        /// </summary>
        /// <remarks>
        /// Values: Lecture, Practical, Laboratory.
        /// </remarks>
        public LessonType LessonTypeId { get; set; }

        /// <summary>
        /// Gets or sets the week pattern.
        /// </summary>
        /// <remarks>
        /// Values: Numerator or Denominator.
        /// </remarks>
        public WeekType WeekType { get; set; }

        /// <summary>
        /// Gets or sets the classroom where the lesson takes place.
        /// </summary>
        public int ClassroomId { get; set; }

        /// <summary>
        /// Gets or sets the staff member teaching this lesson.
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// Gets or sets the parent schedule ID for recurring schedules.
        /// </summary>
        /// <remarks>
        /// Null for initial schedules (numerator/denominator - first and second week).
        /// Non-null for subsequent weeks that follow the parent schedule pattern.
        /// This allows bulk updates to parent schedules while managing exceptions.
        /// </remarks>
        public int? ScheduleParentId { get; set; }

        /// <summary>
        /// Gets or sets the semester this schedule belongs to from that group perspective (so same as the group's SemesterId).
        /// </summary>
        public int SemesterId { get; set; }

        /// <summary>
        /// Gets or sets the date of this schedule entry.
        /// </summary>
        public DateTime ScheduleDate { get; set; }

        // Relationships
        public Subject Subject { get; set; } = null!;
        public TimeTable TimeTable { get; set; } = null!;
        public Classroom Classroom { get; set; } = null!;
        public Lecturer Staff { get; set; } = null!;
        public Schedule? ScheduleParent { get; set; }
        public ICollection<Schedule> ScheduleExceptions { get; set; } = new List<Schedule>();
        public Semester Semester { get; set; } = null!;
        public ICollection<ScheduleGroup> ScheduleGroups { get; set; } = new List<ScheduleGroup>();
    }
}
