using Domain.Enums;

namespace Domain.Entities
{
    public class SubjectClassroom : BaseEntity
    {
        public int SubjectId { get; set; }
        public LessonType LessonType { get; set; }
        public int ClassroomId { get; set; }

        // Relationships
        public Subject Subject { get; set; } = null!;
        public Classroom Classroom { get; set; } = null!;
    }
}
