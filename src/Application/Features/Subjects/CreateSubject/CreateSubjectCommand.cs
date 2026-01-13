using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Subjects.CreateSubject
{
    public record CreateSubjectCommand : IRequest<Result<int>>
    {
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public int SemesterIdFrom { get; init; }
        public int SemesterIdTo { get; init; }
        public int StructureId { get; init; }
        public List<CreateSubjectConfigDto> Configs { get; init; } = new();
    }

    public record CreateSubjectConfigDto
    {
        public LessonType LessonType { get; init; }
        public byte Hours { get; init; }
    }

    public class CreateSubjectValidator : AbstractValidator<CreateSubjectCommand>
    {
        public CreateSubjectValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.SemesterIdFrom).GreaterThan(0);
            RuleFor(x => x.SemesterIdTo).GreaterThan(0);
            RuleFor(x => x.StructureId).GreaterThan(0);

            // At least one config is required
            RuleFor(x => x.Configs).NotEmpty().WithMessage("At least one subject config is required");

            // Validate each config
            RuleForEach(x => x.Configs).ChildRules(config =>
            {
                config.RuleFor(c => c.LessonType).IsInEnum();
                config.RuleFor(c => c.Hours).GreaterThan((byte)0).LessThanOrEqualTo((byte)10);
            });

            // No duplicate lesson types
            RuleFor(x => x.Configs)
                .Must(configs => configs.Select(c => c.LessonType).Distinct().Count() == configs.Count)
                .WithMessage("Duplicate lesson types are not allowed");
        }
    }
}
