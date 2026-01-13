using Domain.Enums;

namespace Application.Features.Groups
{
    public record GroupDto
    {
        public int Id { get; init; }
        public int? ParentId { get; init; }
        public string? ParentName { get; init; }
        public int EducationProgramId { get; init; }
        public string EducationProgramCode { get; init; } = null!;
        public string EducationProgramName { get; init; } = null!;
        public int SemesterId { get; init; }
        public int SemesterNumber { get; init; }
        public LessonType LessonType { get; init; }
        public bool IsActive { get; init; }
        public DateTime StartDate { get; init; }
        public int IndexNumber { get; init; }
        public int? BranchedFromGroupId { get; init; }
        public List<GroupChildDto> Children { get; init; } = new();
    }

    // Lightweight DTO for children to avoid circular references
    public record GroupChildDto
    {
        public int Id { get; init; }
        public LessonType LessonType { get; init; }
        public int IndexNumber { get; init; }
        public bool IsActive { get; init; }
    }

    // Lightweight DTO for list views
    public record GroupListDto
    {
        public int Id { get; init; }
        public int? ParentId { get; init; }
        public int EducationProgramId { get; init; }
        public string EducationProgramCode { get; init; } = null!;
        public int SemesterId { get; init; }
        public int SemesterNumber { get; init; }
        public LessonType LessonType { get; init; }
        public bool IsActive { get; init; }
        public DateTime StartDate { get; init; }
        public int IndexNumber { get; init; }
        public int ChildrenCount { get; init; }
    }
}
