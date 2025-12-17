namespace Domain.Entities
{
    public class Subject : BaseEntity
    {
        public string Code { get; set; } = null!;
        public int SemesterIdFrom { get; set; }
        public int SemesterIdTo { get; set; }
        public int StructureId { get; set; }

        // Relationships
        public Semester SemesterFrom { get; set; } = null!;
        public Semester SemesterTo { get; set; } = null!;
        public Structure Structure { get; set; } = null!;
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ICollection<LecturerSubject> StaffSubjects { get; set; } = new List<LecturerSubject>();
        public ICollection<EducationProgramSubject> EducationProgramSubjects { get; set; } = new List<EducationProgramSubject>();
    }
}
