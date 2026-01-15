namespace Application.Features.LecturerSubjects
{
    public record LecturerSubjectDto
    {
        public int Id { get; init; }
        public int LecturerId { get; init; }
        public int SubjectId { get; init; }
    }

    public record LecturerSubjectListDto
    {
        public int Id { get; init; }
        public int SubjectId { get; init; }
    }

    public record SubjectLecturerListDto
    {
        public int Id { get; init; }
        public int LecturerId { get; init; }
    }
}
