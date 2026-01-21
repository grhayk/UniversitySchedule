using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.GroupSubjectsWithLecturer.GetByGroup
{
    public record GetByGroupQuery : IRequest<Result<List<GroupLecturerSubjectListDto>>>
    {
        public int GroupId { get; init; }
        public LessonType? LessonType { get; init; }
    }

    public class GetByGroupValidator : AbstractValidator<GetByGroupQuery>
    {
        public GetByGroupValidator()
        {
            RuleFor(x => x.GroupId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum().When(x => x.LessonType.HasValue);
        }
    }
}
