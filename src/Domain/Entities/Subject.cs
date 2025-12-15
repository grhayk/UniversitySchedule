namespace Domain.Entities
{
    public class Subject
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public int SemesterIdFrom { get; set; }
        public int SemesterIdTo { get; set; }
        public int StructureId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public Semester SemesterFrom { get; set; } = null!;
        public Semester SemesterTo { get; set; } = null!;
        public Structure Structure { get; set; } = null!;
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ICollection<StaffSubject> StaffSubjects { get; set; } = new List<StaffSubject>();
    }
}
