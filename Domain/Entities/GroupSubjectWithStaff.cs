using Domain.Enums;

namespace Domain.Entities
{
    public class GroupSubjectWithStaff
    {
        public int Id { get; set; }
        public int StaffSubjectId { get; set; }
        public int GroupId { get; set; }
        public int Hours { get; set; }
        public LessonType LessonType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public StaffSubject StaffSubject { get; set; } = null!;
        public Group Group { get; set; } = null!;
    }
}
