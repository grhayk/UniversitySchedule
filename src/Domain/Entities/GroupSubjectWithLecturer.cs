using Domain.Enums;

namespace Domain.Entities
{
    public class GroupSubjectWithLecturer : BaseEntity
    {
        public int LecturerSubjectId { get; set; }
        public int GroupId { get; set; }
        public int Hours { get; set; }
        public LessonType LessonType { get; set; }

        // Relationships
        public LecturerSubject LecturerSubject { get; set; } = null!;
        public Group Group { get; set; } = null!;
    }
}
