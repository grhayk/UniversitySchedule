using Domain.Enums;

namespace Domain.Entities
{
    public class Semester
    {
        public int Id { get; set; }
        public EducationDegree EducationDegree { get; set; }
        public EducationType EducationType { get; set; }
        public int Number { get; set; } // 1-10
        public DateTime CreatedAt { get; set; }

        // Relationships
        public ICollection<Group> Groups { get; set; } = new List<Group>();
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        public ICollection<StudentGroup> StudentGroups { get; set; } = new List<StudentGroup>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
