using FluentValidation;

namespace Application.Features.Students.BulkUpload
{
    public record CsvStudentRecord
    {
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public DateTime BirthDate { get; init; }
        public int GroupId { get; init; }
    }

    public class CsvStudentValidator : AbstractValidator<CsvStudentRecord>
    {
        public CsvStudentValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.BirthDate).NotEmpty().LessThan(DateTime.UtcNow);
            RuleFor(x => x.GroupId).GreaterThan(0);
        }
    }
}
