using Domain.Enums;

namespace Domain.Entities
{
    public class Structure
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Code { get; set; } = null!;
        public StructureType Type { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public Structure? Parent { get; set; }
        public ICollection<Structure> Children { get; set; } = new List<Structure>();
        public ICollection<EducationProgram> EducationPrograms { get; set; } = new List<EducationProgram>();
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
    }
}
