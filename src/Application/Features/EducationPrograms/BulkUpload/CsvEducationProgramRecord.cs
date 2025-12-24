using FluentValidation;

namespace Application.Features.EducationPrograms.BulkUpload
{
    public record CsvEducationProgramRecord
    {
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public int StructureId { get; init; }
        public int? ParentId { get; init; }
    }

    public class CsvEducationProgramValidator : AbstractValidator<CsvEducationProgramRecord>
    {
        public CsvEducationProgramValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.StructureId).GreaterThan(0);
            RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId.HasValue);
        }
    }
}
