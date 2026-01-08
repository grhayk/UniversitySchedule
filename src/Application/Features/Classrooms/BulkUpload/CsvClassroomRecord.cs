using Domain.Enums;
using FluentValidation;

namespace Application.Features.Classrooms.BulkUpload
{
    public record CsvClassroomRecord
    {
        public string Name { get; init; } = null!;
        public int StructureId { get; init; }

        // Characteristics fields (flattened)
        public ClassroomType Type { get; init; }
        public int SeatCapacity { get; init; }
        public bool HasComputer { get; init; }
        public int? ComputerCount { get; init; }
        public bool HasProjector { get; init; }
        public RenovationStatus RenovationStatus { get; init; }
        public BlackboardCondition BlackboardCondition { get; init; }
    }

    public class CsvClassroomValidator : AbstractValidator<CsvClassroomRecord>
    {
        public CsvClassroomValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(20);
            RuleFor(x => x.StructureId).GreaterThan(0);
            RuleFor(x => x.Type).IsInEnum();
            RuleFor(x => x.SeatCapacity)
                .GreaterThan(0).WithMessage("Seat capacity must be greater than 0")
                .LessThanOrEqualTo(500).WithMessage("Seat capacity cannot exceed 500");
            RuleFor(x => x.ComputerCount)
                .GreaterThan(0).WithMessage("Computer count must be greater than 0 if specified")
                .When(x => x.ComputerCount.HasValue);
            RuleFor(x => x.RenovationStatus).IsInEnum();
            RuleFor(x => x.BlackboardCondition).IsInEnum();
        }
    }
}
