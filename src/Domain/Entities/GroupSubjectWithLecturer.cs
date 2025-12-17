using Domain.Enums;

namespace Domain.Entities
{
    public class GroupSubjectWithLecturer : BaseEntity
    {
        public int StaffSubjectId { get; set; }
        public int GroupId { get; set; }
        public int Hours { get; set; }
        public LessonType LessonType { get; set; }

        // Relationships
        public LecturerSubject StaffSubject { get; set; } = null!;
        public Group Group { get; set; } = null!;
    }
}
