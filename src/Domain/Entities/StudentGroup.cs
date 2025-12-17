namespace Domain.Entities
{
    public class StudentGroup : BaseEntity
    {
        public int StudentId { get; set; }
        public int GroupId { get; set; }
        public int SemesterId { get; set; }

        // Relationships
        public Student Student { get; set; } = null!;
        public Group Group { get; set; } = null!;
        public Semester Semester { get; set; } = null!;
    }
}
