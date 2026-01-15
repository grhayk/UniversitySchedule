using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.SubjectClassrooms.RemoveSubjectClassroom
{
    public record RemoveSubjectClassroomCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class RemoveSubjectClassroomValidator : AbstractValidator<RemoveSubjectClassroomCommand>
    {
        public RemoveSubjectClassroomValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
