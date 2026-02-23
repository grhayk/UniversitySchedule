using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.GroupSubjectsWithLecturer.AssignLecturerSubjectToGroup
{
    public record AssignLecturerSubjectToGroupCommand : IRequest<Result<int>>
    {
        public int LecturerSubjectId { get; init; }
        public int? GroupId { get; init; }
        public int? FlowId { get; init; }
        public LessonType LessonType { get; init; }
    }

    public class AssignLecturerSubjectToGroupValidator : AbstractValidator<AssignLecturerSubjectToGroupCommand>
    {
        public AssignLecturerSubjectToGroupValidator()
        {
            RuleFor(x => x.LecturerSubjectId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum();

            // Exactly one of GroupId or FlowId must be provided
            RuleFor(x => x)
                .Must(x => (x.GroupId.HasValue) != (x.FlowId.HasValue))
                .WithMessage("Exactly one of GroupId or FlowId must be provided, not both and not neither.");

            RuleFor(x => x.GroupId).GreaterThan(0).When(x => x.GroupId.HasValue);
            RuleFor(x => x.FlowId).GreaterThan(0).When(x => x.FlowId.HasValue);
        }
    }
}
