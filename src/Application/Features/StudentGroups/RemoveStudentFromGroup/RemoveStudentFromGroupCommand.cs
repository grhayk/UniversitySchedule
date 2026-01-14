using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.StudentGroups.RemoveStudentFromGroup
{
    public record RemoveStudentFromGroupCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class RemoveStudentFromGroupValidator : AbstractValidator<RemoveStudentFromGroupCommand>
    {
        public RemoveStudentFromGroupValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
