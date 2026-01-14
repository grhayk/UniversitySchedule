using FluentValidation;

namespace Application.Features.StudentGroups.BulkUpload
{
    public record CsvStudentGroupRecord
    {
        public int StudentId { get; init; }
        public int GroupId { get; init; }
    }

    public class CsvStudentGroupValidator : AbstractValidator<CsvStudentGroupRecord>
    {
        public CsvStudentGroupValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);
            RuleFor(x => x.GroupId).GreaterThan(0);
        }
    }
}
