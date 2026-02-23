namespace Application.Models.ScheduleGeneration
{
    public class ScheduleGeneratorOutput
    {
        public bool IsFeasible { get; init; }
        public List<PlacedLesson> PlacedLessons { get; init; } = new();
        public string? SolverStatus { get; init; }
    }

    public class PlacedLesson
    {
        public int EventIndex { get; init; }
        public int DemandIndex { get; init; }

        /// <summary>
        /// Absolute timeslot 0-59.
        /// Decode: weekIndex = t / 30, dayIndex = (t % 30) / 6, slotIndex = t % 6.
        /// </summary>
        public int Timeslot { get; init; }

        public int ClassroomId { get; init; }
    }
}
