using Domain.Enums;
using FluentValidation;

namespace Application.Features.GroupSubjectsWithLecturer.BulkUpload
{
    public record CsvGroupSubjectWithLecturerRecord
    {
        public int LecturerSubjectId { get; init; }
        public int GroupId { get; init; }
        public LessonType LessonType { get; init; }
    }

    public class CsvGroupSubjectWithLecturerValidator : AbstractValidator<CsvGroupSubjectWithLecturerRecord>
    {
        public CsvGroupSubjectWithLecturerValidator()
        {
            RuleFor(x => x.LecturerSubjectId).GreaterThan(0);
            RuleFor(x => x.GroupId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum();
        }
    }
}
