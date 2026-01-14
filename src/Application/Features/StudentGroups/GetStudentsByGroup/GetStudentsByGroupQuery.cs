using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.StudentGroups.GetStudentsByGroup
{
    public record GetStudentsByGroupQuery : IRequest<Result<PagedResult<StudentGroupListDto>>>
    {
        public int GroupId { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class GetStudentsByGroupValidator : AbstractValidator<GetStudentsByGroupQuery>
    {
        public GetStudentsByGroupValidator()
        {
            RuleFor(x => x.GroupId).GreaterThan(0);
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        }
    }
}
