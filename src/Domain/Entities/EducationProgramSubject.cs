namespace Domain.Entities
{
    public class EducationProgramSubject : BaseEntity
    {
        public int EducationProgramId { get; set; }
        public int SubjectId { get; set; }
        public int SemesterId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Relationships
        public EducationProgram EducationProgram { get; set; } = null!;
        public Subject Subject { get; set; } = null!;
        public Semester Semester { get; set; } = null!;
    }
}
