using Domain.Enums;

namespace Domain.Entities
{
    public class Flow : BaseEntity
    {
        public int SubjectId { get; set; }
        public LessonType LessonType { get; set; }
        public int SemesterId { get; set; }
        public int StudentsCount { get; set; }
        public bool IsActive { get; set; }

        // Relationships
        public Subject Subject { get; set; } = null!;
        public Semester Semester { get; set; } = null!;
        public ICollection<FlowGroup> FlowGroups { get; set; } = new List<FlowGroup>();
        public ICollection<GroupSubjectWithLecturer> GroupSubjectsWithLecturer { get; set; } = new List<GroupSubjectWithLecturer>();
    }
}
