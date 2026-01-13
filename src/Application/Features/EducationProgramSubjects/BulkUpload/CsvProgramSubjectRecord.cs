using FluentValidation;

namespace Application.Features.EducationProgramSubjects.BulkUpload
{
    public record CsvProgramSubjectRecord
    {
        public int EducationProgramId { get; init; }
        public int SubjectId { get; init; }
        public int SemesterId { get; init; }
        public DateTime FromDate { get; init; }
        public DateTime? ToDate { get; init; }
    }

    public class CsvProgramSubjectValidator : AbstractValidator<CsvProgramSubjectRecord>
    {
        public CsvProgramSubjectValidator()
        {
            RuleFor(x => x.EducationProgramId).GreaterThan(0);
            RuleFor(x => x.SubjectId).GreaterThan(0);
            RuleFor(x => x.SemesterId).GreaterThan(0);
            RuleFor(x => x.FromDate).NotEmpty();
            RuleFor(x => x.ToDate)
                .GreaterThan(x => x.FromDate)
                .When(x => x.ToDate.HasValue)
                .WithMessage("ToDate must be after FromDate");
        }
    }
}
