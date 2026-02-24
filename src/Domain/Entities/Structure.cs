using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Represents an organizational unit within the university.
    /// </summary>
    /// <remarks>
    /// Structures can be hierarchical (University → Institute → Chair).
    /// This is the base organizational level for grouping education programs,
    /// subjects, and classrooms.
    /// </remarks>
    public class Structure : BaseEntity
    {
        /// <summary>
        /// Gets or sets the parent structure ID for hierarchical relationships.
        /// </summary>
        /// <remarks>
        /// Null if this is a top-level structure (e.g., University).
        /// </remarks>
        public int? ParentId { get; set; }
        public string Code { get; set; } = null!;

        /// <summary>
        /// Gets or sets the type of structure.
        /// </summary>
        /// <remarks>
        /// Values: University, Institute, or Chair.
        /// </remarks>
        public StructureType Type { get; set; }

        /// <summary>
        /// Gets or sets the date when this structure was established.
        /// </summary>
        public DateTime FromDate { get; set; }

        /// <summary>
        /// Gets or sets the date when this structure was dissolved (if applicable).
        /// </summary>
        public DateTime? ToDate { get; set; }

        // Relationships
        public Structure? Parent { get; set; }
        public ICollection<Structure> Children { get; set; } = new List<Structure>();

        /// <summary>
        /// Gets or sets the education programs offered by this structure. Education programs belong to chairs.
        /// </summary>
        public ICollection<EducationProgram> EducationPrograms { get; set; } = new List<EducationProgram>();

        /// <summary>
        /// Gets or sets the subjects taught within this structure. Subjects belong to chairs.
        /// </summary>
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
    }
}
