using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.StudentGroups.AssignStudentToGroup
{
    public record AssignStudentToGroupCommand : IRequest<Result<int>>
    {
        public int StudentId { get; init; }
        public int GroupId { get; init; }
    }

    public class AssignStudentToGroupValidator : AbstractValidator<AssignStudentToGroupCommand>
    {
        public AssignStudentToGroupValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);
            RuleFor(x => x.GroupId).GreaterThan(0);
        }
    }
}
