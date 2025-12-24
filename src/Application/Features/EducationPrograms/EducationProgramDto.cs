namespace Application.Features.EducationPrograms
{
    public record EducationProgramDto
    {
        public int Id { get; init; }
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public int StructureId { get; init; }
        public int? ParentId { get; init; }
    }
}
