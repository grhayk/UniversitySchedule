namespace Domain.Entities
{
    public class EducationProgram : BaseEntity
    {
        public string Code { get; set; } = null!;
        public int StructureId { get; set; }
        public int? ParentId { get; set; }

        // Relationships
        public Structure Structure { get; set; } = null!;
        public EducationProgram? Parent { get; set; }
        public ICollection<EducationProgram> Children { get; set; } = new List<EducationProgram>();
        public ICollection<Group> Groups { get; set; } = new List<Group>();
        public ICollection<EducationProgramSubject> EducationProgramSubjects { get; set; } = new List<EducationProgramSubject>();
    }
}
