using Domain.Enums;

namespace Application.Features.Classrooms
{
    public record ClassroomDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public int StructureId { get; init; }
        public ClassroomCharacteristicsDto Characteristics { get; init; } = null!;
    }

    public record ClassroomCharacteristicsDto
    {
        public ClassroomType Type { get; init; }
        public int SeatCapacity { get; init; }
        public bool HasComputer { get; init; }
        public int? ComputerCount { get; init; }
        public bool HasProjector { get; init; }
        public RenovationStatus RenovationStatus { get; init; }
        public BlackboardCondition BlackboardCondition { get; init; }
    }
}
