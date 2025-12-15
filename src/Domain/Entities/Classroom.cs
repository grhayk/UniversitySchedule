namespace Domain.Entities
{
    public class Classroom
    {
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the classroom name or number (e.g., "5113").
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets the structure this classroom belongs to.
        /// </summary>
        /// <remarks>
        /// Null if the classroom is not assigned to a specific structure.
        /// </remarks>
        public int? StructureId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public Structure? Structure { get; set; }

        /// <summary>
        /// Gets or sets the detailed characteristics of this classroom.
        /// </summary>
        public ClassroomCharacteristics? Characteristics { get; set; }
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
