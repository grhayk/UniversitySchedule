using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.SubjectClassrooms.GetSubjectsByClassroom
{
    public record GetSubjectsByClassroomQuery : IRequest<Result<List<ClassroomSubjectListDto>>>
    {
        public int ClassroomId { get; init; }
        public LessonType? LessonType { get; init; }
    }

    public class GetSubjectsByClassroomValidator : AbstractValidator<GetSubjectsByClassroomQuery>
    {
        public GetSubjectsByClassroomValidator()
        {
            RuleFor(x => x.ClassroomId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum().When(x => x.LessonType.HasValue);
        }
    }
}
