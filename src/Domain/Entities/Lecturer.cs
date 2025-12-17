namespace Domain.Entities
{
    public class Lecturer : Person
    {
        // Relationships
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ICollection<LecturerSubject> StaffSubjects { get; set; } = new List<LecturerSubject>();
    }
}
