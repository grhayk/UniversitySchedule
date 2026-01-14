namespace Application.Features.Lecturers
{
    public record LecturerDto
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public DateTime BirthDate { get; init; }
        public int StructureId { get; init; }
    }

    public record LecturerListDto
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public int StructureId { get; init; }
    }
}
