using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Represents detailed characteristics of a classroom.
    /// </summary>
    /// <remarks>
    /// These characteristics determine the classroom's suitability
    /// for specific types of lessons and subjects.
    /// </remarks>
    public class ClassroomCharacteristics
    {
        public int Id { get; set; }
        public int ClassroomId { get; set; }

        /// <summary>
        /// Gets or sets the classroom type.
        /// </summary>
        /// <remarks>
        /// Values: Laboratory, Chair, StudentCouncil, DirectClassroom, Cabinet,
        /// DirectorOffice, Hall, ChairRoom, ATS, Teleconference.
        /// </remarks>
        public ClassroomType Type { get; set; }
        public int SeatCapacity { get; set; } // e.g., 30, 50, 100
        public bool HasComputer { get; set; }
        public int? ComputerCount { get; set; }
        public bool HasProjector { get; set; }

        /// <summary>
        /// Gets or sets the renovation status of this classroom.
        /// </summary>
        /// <remarks>
        /// Values: Good or NeedsRenovation.
        /// </remarks>
        public RenovationStatus RenovationStatus { get; set; }

        /// <summary>
        /// Gets or sets the condition of the blackboard (if available).
        /// </summary>
        /// <remarks>
        /// Values: NotAvailable, Poor, Good.
        /// </remarks>
        public BlackboardCondition BlackboardCondition { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public Classroom Classroom { get; set; } = null!;
    }
}
