using FluentValidation;

namespace Application.Features.Subjects.BulkUpload
{
    public record CsvSubjectRecord
    {
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public int SemesterIdFrom { get; init; }
        public int SemesterIdTo { get; init; }
        public int StructureId { get; init; }

        // Flattened config fields (0 or null means no config for that type)
        public byte? LectureHours { get; init; }
        public byte? PracticalHours { get; init; }
        public byte? LaboratoryHours { get; init; }
    }

    public class CsvSubjectValidator : AbstractValidator<CsvSubjectRecord>
    {
        public CsvSubjectValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.SemesterIdFrom).GreaterThan(0);
            RuleFor(x => x.SemesterIdTo).GreaterThan(0);
            RuleFor(x => x.StructureId).GreaterThan(0);

            // At least one config must have hours > 0
            RuleFor(x => x)
                .Must(x => (x.LectureHours ?? 0) > 0 || (x.PracticalHours ?? 0) > 0 || (x.LaboratoryHours ?? 0) > 0)
                .WithMessage("At least one lesson type must have hours greater than 0");

            // Hours validation (when specified)
            RuleFor(x => x.LectureHours)
                .LessThanOrEqualTo((byte)10).WithMessage("Lecture hours cannot exceed 10")
                .When(x => x.LectureHours.HasValue && x.LectureHours > 0);

            RuleFor(x => x.PracticalHours)
                .LessThanOrEqualTo((byte)10).WithMessage("Practical hours cannot exceed 10")
                .When(x => x.PracticalHours.HasValue && x.PracticalHours > 0);

            RuleFor(x => x.LaboratoryHours)
                .LessThanOrEqualTo((byte)10).WithMessage("Laboratory hours cannot exceed 10")
                .When(x => x.LaboratoryHours.HasValue && x.LaboratoryHours > 0);
        }
    }
}
