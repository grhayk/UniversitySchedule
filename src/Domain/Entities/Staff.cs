namespace Domain.Entities
{
    public class Staff
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ICollection<StaffSubject> StaffSubjects { get; set; } = new List<StaffSubject>();
    }
}
