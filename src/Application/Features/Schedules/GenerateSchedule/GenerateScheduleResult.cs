namespace Application.Features.Schedules.GenerateSchedule
{
    public record GenerateScheduleResult
    {
        public int TotalSchedulesCreated { get; init; }
        public int TotalDemandsProcessed { get; init; }
        public string SolverStatus { get; init; } = null!;
    }
}
