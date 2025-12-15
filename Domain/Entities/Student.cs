using Domain.Enums;

namespace Domain.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int StructureId { get; set; }
        public EducationDegree EducationDegree { get; set; }
        public EducationType EducationType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public Group Group { get; set; } = null!;
        public Structure Structure { get; set; } = null!;
        public ICollection<StudentGroup> StudentGroups { get; set; } = new List<StudentGroup>();
    }
}
