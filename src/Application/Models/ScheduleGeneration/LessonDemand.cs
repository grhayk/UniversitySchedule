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
        /// Valid classroom IDs filtered by SeatCapacity.
        /// From SubjectClassroom if exists, otherwise same-building + university-level classrooms.
        /// </summary>
        public List<int> ValidClassroomIds { get; init; } = new();

        /// <summary>
        /// Preferred classroom IDs (same building as the group's chair).
        /// Subset of ValidClassroomIds. Empty if from SubjectClassroom or no building info.
        /// </summary>
        public List<int> PreferredClassroomIds { get; init; } = new();

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
