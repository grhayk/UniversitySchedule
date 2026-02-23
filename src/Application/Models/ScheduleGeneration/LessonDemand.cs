using Domain.Enums;

namespace Application.Models.ScheduleGeneration
{
    /// <summary>
    /// One teaching demand resolved from GroupSubjectWithLecturer with all data the solver needs.
    /// </summary>
    public class LessonDemand
    {
        public int GroupSubjectWithLecturerId { get; init; }
        public int LecturerId { get; init; }
        public int SubjectId { get; init; }
        public LessonType LessonType { get; init; }
        public int Hours { get; init; }

        /// <summary>
        /// All group IDs that become busy when this demand is scheduled.
        /// Expanded with parent/child/flow logic.
        /// </summary>
        public List<int> BusyGroupIds { get; init; } = new();

        /// <summary>
        /// Valid classroom IDs from SubjectClassroom filtered by SeatCapacity.
        /// </summary>
        public List<int> ValidClassroomIds { get; init; } = new();

        /// <summary>
        /// Student count for capacity checking.
        /// </summary>
        public int StudentCount { get; init; }

        /// <summary>
        /// Group IDs to store in ScheduleGroup (the direct group or flow groups).
        /// </summary>
        public List<int> ScheduleGroupIds { get; init; } = new();

        /// <summary>
        /// The SemesterId for the Schedule entity (from the group's semester).
        /// </summary>
        public int SemesterId { get; init; }
    }
}
