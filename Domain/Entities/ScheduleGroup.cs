namespace Domain.Entities
{
    public class ScheduleGroup
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public int GroupId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Relationships
        public Schedule Schedule { get; set; } = null!;
        public Group Group { get; set; } = null!;
    }
}
