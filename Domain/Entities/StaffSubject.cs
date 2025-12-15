namespace Domain.Entities
{
    public class StaffSubject
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public int SubjectId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public Staff Staff { get; set; } = null!;
        public Subject Subject { get; set; } = null!;
        public ICollection<GroupSubjectWithStaff> GroupSubjectsWithStaff { get; set; } = new List<GroupSubjectWithStaff>();
    }
}
