namespace Application.Models.ScheduleGeneration
{
    public class ScheduleGeneratorInput
    {
        public List<LessonDemand> Demands { get; init; } = new();
        public List<LessonEvent> Events { get; init; } = new();

        /// <summary>
        /// All classroom IDs referenced by any demand.
        /// </summary>
        public List<int> AllClassroomIds { get; init; } = new();

        public int SlotsPerDay { get; init; } = 6;
        public int DaysPerWeek { get; init; } = 5;

        /// <summary>
        /// Timeslots per week = DaysPerWeek * SlotsPerDay (default 30).
        /// </summary>
        public int TimeslotsPerWeek => DaysPerWeek * SlotsPerDay;

        /// <summary>
        /// Total timeslots across 2 weeks (default 60).
        /// </summary>
        public int TotalTimeslots => TimeslotsPerWeek * 2;

        /// <summary>
        /// Solver time limit in seconds.
        /// </summary>
        public double TimeLimitSeconds { get; init; } = 60;
    }
}
