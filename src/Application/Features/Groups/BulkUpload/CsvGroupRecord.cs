using Domain.Enums;
using FluentValidation;

namespace Application.Features.Groups.BulkUpload
{
    public record CsvGroupRecord
    {
        public int? ParentId { get; init; }
        public int EducationProgramId { get; init; }
        public int SemesterId { get; init; }
        public LessonType LessonType { get; init; }
        public bool IsActive { get; init; }
        public DateTime StartDate { get; init; }
        public int IndexNumber { get; init; }
        public int? BranchedFromGroupId { get; init; }
    }

    public class CsvGroupValidator : AbstractValidator<CsvGroupRecord>
    {
        public CsvGroupValidator()
        {
            RuleFor(x => x.EducationProgramId).GreaterThan(0);
            RuleFor(x => x.SemesterId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum();
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.IndexNumber).GreaterThan(0);

            RuleFor(x => x.ParentId)
                .GreaterThan(0)
                .When(x => x.ParentId.HasValue);

            RuleFor(x => x.BranchedFromGroupId)
                .GreaterThan(0)
                .When(x => x.BranchedFromGroupId.HasValue);

            // Main groups (no parent) should be Lecture type
            RuleFor(x => x.LessonType)
                .Equal(LessonType.Lecture)
                .When(x => !x.ParentId.HasValue)
                .WithMessage("Main groups (without parent) must have LessonType = Lecture");

            // Subgroups (with parent) should not be Lecture type
            RuleFor(x => x.LessonType)
                .NotEqual(LessonType.Lecture)
                .When(x => x.ParentId.HasValue)
                .WithMessage("Subgroups (with parent) cannot have LessonType = Lecture");
        }
    }
}
