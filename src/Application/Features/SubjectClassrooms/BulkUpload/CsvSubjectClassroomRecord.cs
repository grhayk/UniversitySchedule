using Domain.Enums;
using FluentValidation;

namespace Application.Features.SubjectClassrooms.BulkUpload
{
    public record CsvSubjectClassroomRecord
    {
        public int SubjectId { get; init; }
        public LessonType LessonType { get; init; }
        public int ClassroomId { get; init; }
    }

    public class CsvSubjectClassroomValidator : AbstractValidator<CsvSubjectClassroomRecord>
    {
        public CsvSubjectClassroomValidator()
        {
            RuleFor(x => x.SubjectId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum();
            RuleFor(x => x.ClassroomId).GreaterThan(0);
        }
    }
}
