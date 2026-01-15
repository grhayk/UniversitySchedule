using Domain.Enums;

namespace Application.Features.SubjectClassrooms
{
    public record SubjectClassroomDto
    {
        public int Id { get; init; }
        public int SubjectId { get; init; }
        public LessonType LessonType { get; init; }
        public int ClassroomId { get; init; }
    }

    // For querying classrooms by subject
    public record SubjectClassroomListDto
    {
        public int Id { get; init; }
        public int ClassroomId { get; init; }
        public LessonType LessonType { get; init; }
    }

    // For querying subjects by classroom
    public record ClassroomSubjectListDto
    {
        public int Id { get; init; }
        public int SubjectId { get; init; }
        public LessonType LessonType { get; init; }
    }
}
