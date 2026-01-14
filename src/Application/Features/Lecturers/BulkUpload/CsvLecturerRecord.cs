using FluentValidation;

namespace Application.Features.Lecturers.BulkUpload
{
    public record CsvLecturerRecord
    {
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public DateTime BirthDate { get; init; }
        public int StructureId { get; init; }
    }

    public class CsvLecturerValidator : AbstractValidator<CsvLecturerRecord>
    {
        public CsvLecturerValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.BirthDate).NotEmpty().LessThan(DateTime.UtcNow);
            RuleFor(x => x.StructureId).GreaterThan(0);
        }
    }
}
