namespace Domain.Entities
{
    public class Classroom
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // e.g., "5113"
        public int? StructureId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public Structure? Structure { get; set; }
        public ClassroomCharacteristics? Characteristics { get; set; }
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
