using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Represents subject's configs.
    /// </summary>
    /// <remarks>
    /// Subjects can have multiple configurations defining how many hours are allocated for each lesson type (Lecture, Practical, Laboratory).
    /// Not must for subject to have all lesson types (Considers 0 hours for that lesson type).
    /// </remarks>
    public class SubjectConfig : BaseEntity
    {
        /// <summary>
        /// Gets or sets the subject ID this configuration applies to.
        /// </summary>
        public int SubjectId { get; set; }

        /// <summary>
        /// Gets or sets the type of lessons subject carries.
        /// </summary>
        public LessonType LessonType { get; set; }

        /// <summary>
        /// Gets or sets the number of hours allocated for this subject and lesson type.
        /// </summary>
        /// <remarks>
        /// If Hours is 1, it means that subject with that lesson type has 1 lesson entry per 2 weeks in the schedule.(either numerator or denominator week)
        /// If Hours is 2, it means that subject with that lesson type has 2 lesson entries per 2 weeks in the schedule.(1 in numerator and 1 in denominator week)
        /// If Hours is 3, it means that subject with that lesson type has 3 lesson entries per 2 weeks in the schedule.(2 in numerator and 1 in denominator week or vice versa)
        /// </remarks>
        public byte Hours { get; set; }

        // Relationships
        public Subject Subject { get; set; } = null!;
    }
}
