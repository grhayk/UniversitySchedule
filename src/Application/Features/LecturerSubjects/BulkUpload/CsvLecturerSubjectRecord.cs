using FluentValidation;

namespace Application.Features.LecturerSubjects.BulkUpload
{
    public record CsvLecturerSubjectRecord
    {
        public int LecturerId { get; init; }
        public int SubjectId { get; init; }
    }

    public class CsvLecturerSubjectValidator : AbstractValidator<CsvLecturerSubjectRecord>
    {
        public CsvLecturerSubjectValidator()
        {
            RuleFor(x => x.LecturerId).GreaterThan(0);
            RuleFor(x => x.SubjectId).GreaterThan(0);
        }
    }
}
