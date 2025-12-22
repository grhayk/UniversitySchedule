namespace Domain.Entities
{
    public class Classroom : BaseEntity
    {
        /// <summary>
        /// Gets or sets the classroom name or number (e.g., "5113").
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets the structure this classroom belongs to.
        /// </summary>
        public int StructureId { get; set; }

        // Relationships
        public Structure Structure { get; set; } = null!;

        /// <summary>
        /// Gets or sets the detailed characteristics of this classroom.
        /// </summary>
        public ClassroomCharacteristics? Characteristics { get; set; }
        public ICollection<SubjectClassroom> SubjectClassrooms { get; set; } = new List<SubjectClassroom>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
