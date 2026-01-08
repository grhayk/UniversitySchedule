using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Classrooms.GetClassroom
{
    public record GetClassroomQuery : IRequest<Result<ClassroomDto>>
    {
        public int Id { get; init; }
    }

    public class GetClassroomValidator : AbstractValidator<GetClassroomQuery>
    {
        public GetClassroomValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
