using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.GroupSubjectsWithLecturer.GetByFlow
{
    public record GetByFlowQuery : IRequest<Result<List<GroupLecturerSubjectListDto>>>
    {
        public int FlowId { get; init; }
        public LessonType? LessonType { get; init; }
    }

    public class GetByFlowValidator : AbstractValidator<GetByFlowQuery>
    {
        public GetByFlowValidator()
        {
            RuleFor(x => x.FlowId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum().When(x => x.LessonType.HasValue);
        }
    }
}
