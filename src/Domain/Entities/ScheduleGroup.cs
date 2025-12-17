namespace Domain.Entities
{
    public class ScheduleGroup : BaseEntity
    {
        public int ScheduleId { get; set; }
        public int GroupId { get; set; }

        // Relationships
        public Schedule Schedule { get; set; } = null!;
        public Group Group { get; set; } = null!;
    }
}
