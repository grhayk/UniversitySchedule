using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Groups.GetAllGroups
{
    public record GetAllGroupsQuery : IRequest<Result<PagedResult<GroupListDto>>>
    {
        public int? EducationProgramId { get; init; }
        public int? SemesterId { get; init; }
        public LessonType? LessonType { get; init; }
        public bool? IsActive { get; init; }
        public bool? MainGroupsOnly { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class GetAllGroupsValidator : AbstractValidator<GetAllGroupsQuery>
    {
        public GetAllGroupsValidator()
        {
            RuleFor(x => x.EducationProgramId).GreaterThan(0).When(x => x.EducationProgramId.HasValue);
            RuleFor(x => x.SemesterId).GreaterThan(0).When(x => x.SemesterId.HasValue);
            RuleFor(x => x.LessonType).IsInEnum().When(x => x.LessonType.HasValue);
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        }
    }
}
