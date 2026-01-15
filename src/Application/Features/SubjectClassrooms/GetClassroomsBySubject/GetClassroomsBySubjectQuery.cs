using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.SubjectClassrooms.GetClassroomsBySubject
{
    public record GetClassroomsBySubjectQuery : IRequest<Result<List<SubjectClassroomListDto>>>
    {
        public int SubjectId { get; init; }
        public LessonType? LessonType { get; init; }
    }

    public class GetClassroomsBySubjectValidator : AbstractValidator<GetClassroomsBySubjectQuery>
    {
        public GetClassroomsBySubjectValidator()
        {
            RuleFor(x => x.SubjectId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum().When(x => x.LessonType.HasValue);
        }
    }
}
