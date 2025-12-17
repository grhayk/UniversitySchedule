using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Represents an academic semester.
    /// </summary>
    /// <remarks>
    /// Semesters are defined by education degree, education type, and semester number.
    /// For example: Bachelor's program, Stationary education, Semester 1.
    /// </remarks>
    public class Semester : BaseEntity
    {
        /// <summary>
        /// Gets or sets the education degree level.
        /// </summary>
        /// <remarks>
        /// Values: Bachelor, Master, PhD.
        /// </remarks>
        public EducationDegree EducationDegree { get; set; }

        /// <summary>
        /// Gets or sets the education delivery type.
        /// </summary>
        /// <remarks>
        /// Values: Stationary or Remote.
        /// </remarks>
        public EducationType EducationType { get; set; }

        /// <summary>
        /// Gets or sets the semester number (1-10). Remote education has 10 semesters.
        /// </summary>
        public int Number { get; set; }

        // Relationships
        /// <summary>
        /// Gets or sets the groups enrolled in this semester.
        /// </summary>
        public ICollection<Group> Groups { get; set; } = new List<Group>();

        /// <summary>
        /// Gets or sets the subjects scheduled for this semester.
        /// </summary>
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        public ICollection<StudentGroup> StudentGroups { get; set; } = new List<StudentGroup>();

        /// <summary>
        /// Gets or sets the schedules for this semester.
        /// </summary>
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ICollection<EducationProgramSubject> EducationProgramSubjects { get; set; } = new List<EducationProgramSubject>();
    }
}
