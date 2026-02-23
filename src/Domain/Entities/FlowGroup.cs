namespace Domain.Entities
{
    public class FlowGroup : BaseEntity
    {
        public int FlowId { get; set; }
        public int GroupId { get; set; }

        // Relationships
        public Flow Flow { get; set; } = null!;
        public Group Group { get; set; } = null!;
    }
}
