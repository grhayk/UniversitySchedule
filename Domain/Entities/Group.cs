using Domain.Enums;

namespace Domain.Entities
{
    public class Group
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int EducationProgramId { get; set; }
        public int SemesterId { get; set; }
        public LessonType LessonType { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public int IndexNumber { get; set; }
        public int? BranchedFromGroupId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public Group? Parent { get; set; }
        public Group? BranchedFromGroup { get; set; }
        public ICollection<Group> Children { get; set; } = new List<Group>();
        public EducationProgram EducationProgram { get; set; } = null!;
        public Semester Semester { get; set; } = null!;
        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<StudentGroup> StudentGroups { get; set; } = new List<StudentGroup>();
        public ICollection<ScheduleGroup> ScheduleGroups { get; set; } = new List<ScheduleGroup>();
        public ICollection<GroupSubjectWithStaff> GroupSubjectsWithStaff { get; set; } = new List<GroupSubjectWithStaff>();
    }
}
