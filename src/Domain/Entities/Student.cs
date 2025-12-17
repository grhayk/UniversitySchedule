using Domain.Enums;

namespace Domain.Entities
{
    public class Student : Person
    {
        public int? GroupId { get; set; }
        public EducationDegree? EducationDegree { get; set; } // Nullable for lecturers, TPH same table
        public EducationType? EducationType { get; set; }

        // Relationships
        public Group Group { get; set; } = null!;
        public ICollection<StudentGroup> StudentGroups { get; set; } = new List<StudentGroup>();
    }
}
