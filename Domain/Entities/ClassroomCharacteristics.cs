using Domain.Enums;

namespace Domain.Entities
{
    public class ClassroomCharacteristics
    {
        public int Id { get; set; }
        public int ClassroomId { get; set; }
        public ClassroomType Type { get; set; }
        public int SeatCapacity { get; set; } // e.g., 30, 50, 100
        public bool HasComputer { get; set; }
        public int? ComputerCount { get; set; }
        public bool HasProjector { get; set; }
        public RenovationStatus RenovationStatus { get; set; }
        public BlackboardCondition BlackboardCondition { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public Classroom Classroom { get; set; } = null!;
    }
}
