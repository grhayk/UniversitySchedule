using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.SubjectClassrooms.AssignClassroomToSubject
{
    public record AssignClassroomToSubjectCommand : IRequest<Result<int>>
    {
        public int SubjectId { get; init; }
        public LessonType LessonType { get; init; }
        public int ClassroomId { get; init; }
    }

    public class AssignClassroomToSubjectValidator : AbstractValidator<AssignClassroomToSubjectCommand>
    {
        public AssignClassroomToSubjectValidator()
        {
            RuleFor(x => x.SubjectId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum();
            RuleFor(x => x.ClassroomId).GreaterThan(0);
        }
    }
}
