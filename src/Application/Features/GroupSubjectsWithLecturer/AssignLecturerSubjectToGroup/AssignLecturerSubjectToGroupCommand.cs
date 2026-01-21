using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.GroupSubjectsWithLecturer.AssignLecturerSubjectToGroup
{
    public record AssignLecturerSubjectToGroupCommand : IRequest<Result<int>>
    {
        public int LecturerSubjectId { get; init; }
        public int GroupId { get; init; }
        public LessonType LessonType { get; init; }
    }

    public class AssignLecturerSubjectToGroupValidator : AbstractValidator<AssignLecturerSubjectToGroupCommand>
    {
        public AssignLecturerSubjectToGroupValidator()
        {
            RuleFor(x => x.LecturerSubjectId).GreaterThan(0);
            RuleFor(x => x.GroupId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum();
        }
    }
}
