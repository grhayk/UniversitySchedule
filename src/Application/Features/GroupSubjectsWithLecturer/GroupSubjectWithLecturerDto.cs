using Domain.Enums;

namespace Application.Features.GroupSubjectsWithLecturer
{
    public record GroupSubjectWithLecturerDto
    {
        public int Id { get; init; }
        public int LecturerSubjectId { get; init; }
        public int GroupId { get; init; }
        public int Hours { get; init; }
        public LessonType LessonType { get; init; }
    }

    // For querying by group
    public record GroupLecturerSubjectListDto
    {
        public int Id { get; init; }
        public int LecturerSubjectId { get; init; }
        public int Hours { get; init; }
        public LessonType LessonType { get; init; }
    }

    // For querying by lecturer-subject
    public record LecturerSubjectGroupListDto
    {
        public int Id { get; init; }
        public int GroupId { get; init; }
        public int Hours { get; init; }
        public LessonType LessonType { get; init; }
    }
}
