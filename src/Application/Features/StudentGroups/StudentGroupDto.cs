using Domain.Enums;

namespace Application.Features.StudentGroups
{
    public record StudentGroupDto
    {
        public int Id { get; init; }
        public int StudentId { get; init; }
        public int GroupId { get; init; }
        public int SemesterId { get; init; }
        public LessonType GroupLessonType { get; init; }
        public int GroupIndexNumber { get; init; }
    }

    public record StudentGroupListDto
    {
        public int Id { get; init; }
        public int StudentId { get; init; }
        public string StudentFirstName { get; init; } = null!;
        public string StudentLastName { get; init; } = null!;
        public int GroupId { get; init; }
        public LessonType GroupLessonType { get; init; }
        public int GroupIndexNumber { get; init; }
    }
}
