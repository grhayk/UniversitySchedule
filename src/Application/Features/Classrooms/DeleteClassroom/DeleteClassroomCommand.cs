using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Classrooms.DeleteClassroom
{
    public record DeleteClassroomCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class DeleteClassroomValidator : AbstractValidator<DeleteClassroomCommand>
    {
        public DeleteClassroomValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
