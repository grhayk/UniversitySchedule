using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a group of students.
    /// </summary>
    /// <remarks>
    /// Groups can be hierarchical (main group → subgroups for different lesson types).
    /// Each group is associated with a semester.
    /// </remarks>
    public class Group : BaseEntity
    {
        /// <summary>
        /// Gets or sets the parent group ID for hierarchical relationships.
        /// </summary>
        /// <remarks>
        /// Null if this is a main group. Non-null for subgroups (e.g., lecture group, practical group).
        /// </remarks>
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the education program this group follows.
        /// </summary>
        public int EducationProgramId { get; set; }
        public int SemesterId { get; set; }

        /// <summary>
        /// Gets or sets the type of lessons (if this is a subgroup).
        /// </summary>
        /// <remarks>
        /// Values: Lecture, Practical, Laboratory.
        /// Lecture for main groups.
        /// </remarks>
        public LessonType LessonType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this group is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the start date of the group.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the group index number is it 1st Lab or 2nd Practical.(1,2,3 etc.)
        /// </summary>
        public int IndexNumber { get; set; }

        /// <summary>
        /// Gets or sets a group id from which this group was branched. Can happen after 6th semester for certain education program groups.
        /// </summary>
        public int? BranchedFromGroupId { get; set; }

        // Relationships
        public Group? Parent { get; set; }
        public Group? BranchedFromGroup { get; set; }
        public ICollection<Group> Children { get; set; } = new List<Group>();
        public EducationProgram EducationProgram { get; set; } = null!;
        public Semester Semester { get; set; } = null!;
        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<StudentGroup> StudentGroups { get; set; } = new List<StudentGroup>();
        public ICollection<ScheduleGroup> ScheduleGroups { get; set; } = new List<ScheduleGroup>();
        public ICollection<GroupSubjectWithLecturer> GroupSubjectsWithLecturer { get; set; } = new List<GroupSubjectWithLecturer>();
    }
}
