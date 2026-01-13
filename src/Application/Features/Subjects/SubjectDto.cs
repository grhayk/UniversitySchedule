using Domain.Enums;

namespace Application.Features.Subjects
{
    public record SubjectDto
    {
        public int Id { get; init; }
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public int SemesterIdFrom { get; init; }
        public int SemesterIdTo { get; init; }
        public int StructureId { get; init; }
        public List<SubjectConfigDto> Configs { get; init; } = new();
    }

    public record SubjectConfigDto
    {
        public LessonType LessonType { get; init; }
        public byte Hours { get; init; }
    }
}
