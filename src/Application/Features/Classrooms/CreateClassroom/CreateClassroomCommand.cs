using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Classrooms.CreateClassroom
{
    public record CreateClassroomCommand : IRequest<Result<int>>
    {
        public string Name { get; init; } = null!;
        public int StructureId { get; init; }
        public CreateClassroomCharacteristicsDto Characteristics { get; init; } = null!;
    }

    public record CreateClassroomCharacteristicsDto
    {
        public ClassroomType Type { get; init; }
        public int SeatCapacity { get; init; }
        public bool HasComputer { get; init; }
        public int? ComputerCount { get; init; }
        public bool HasProjector { get; init; }
        public RenovationStatus RenovationStatus { get; init; }
        public BlackboardCondition BlackboardCondition { get; init; }
    }

    public class CreateClassroomValidator : AbstractValidator<CreateClassroomCommand>
    {
        public CreateClassroomValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(20);
            RuleFor(x => x.StructureId).GreaterThan(0);

            // Characteristics are REQUIRED
            RuleFor(x => x.Characteristics).NotNull().WithMessage("Characteristics are required");
            RuleFor(x => x.Characteristics.Type).IsInEnum();
            RuleFor(x => x.Characteristics.SeatCapacity)
                .GreaterThan(0).WithMessage("Seat capacity must be greater than 0")
                .LessThanOrEqualTo(500).WithMessage("Seat capacity cannot exceed 500");
            RuleFor(x => x.Characteristics.ComputerCount)
                .GreaterThan(0).WithMessage("Computer count must be greater than 0 if specified")
                .When(x => x.Characteristics.ComputerCount.HasValue);
            RuleFor(x => x.Characteristics.RenovationStatus).IsInEnum();
            RuleFor(x => x.Characteristics.BlackboardCondition).IsInEnum();
        }
    }
}
