namespace Application.Models.ScheduleGeneration
{
    /// <summary>
    /// A single lesson occurrence to place in exactly one timeslot + classroom.
    /// Multiple events are generated from a single LessonDemand based on Hours.
    /// </summary>
    public class LessonEvent
    {
        public int EventIndex { get; init; }
        public int DemandIndex { get; init; }
    }
}
