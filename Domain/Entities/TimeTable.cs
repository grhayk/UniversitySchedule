namespace Domain.Entities
{
    public class TimeTable
    {
        public int Id { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public DateTime CreatedAt { get; set; }

        // Relationships
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
