namespace Application.Features.EducationProgramSubjects
{
    public record EducationProgramSubjectDto
    {
        public int Id { get; init; }
        public int EducationProgramId { get; init; }
        public string EducationProgramCode { get; init; } = null!;
        public string EducationProgramName { get; init; } = null!;
        public int SubjectId { get; init; }
        public string SubjectCode { get; init; } = null!;
        public string SubjectName { get; init; } = null!;
        public int SemesterId { get; init; }
        public int SemesterNumber { get; init; }
        public DateTime FromDate { get; init; }
        public DateTime? ToDate { get; init; }
    }

    // Lightweight DTO when querying subjects by program
    public record ProgramSubjectDto
    {
        public int Id { get; init; }
        public int SubjectId { get; init; }
        public string SubjectCode { get; init; } = null!;
        public string SubjectName { get; init; } = null!;
        public int SemesterId { get; init; }
        public int SemesterNumber { get; init; }
        public DateTime FromDate { get; init; }
        public DateTime? ToDate { get; init; }
    }

    // Lightweight DTO when querying programs by subject
    public record SubjectProgramDto
    {
        public int Id { get; init; }
        public int EducationProgramId { get; init; }
        public string EducationProgramCode { get; init; } = null!;
        public string EducationProgramName { get; init; } = null!;
        public int SemesterId { get; init; }
        public int SemesterNumber { get; init; }
        public DateTime FromDate { get; init; }
        public DateTime? ToDate { get; init; }
    }
}
