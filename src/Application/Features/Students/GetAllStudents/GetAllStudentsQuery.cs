using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Students.GetAllStudents
{
    public record GetAllStudentsQuery : IRequest<Result<PagedResult<StudentListDto>>>
    {
        public int? GroupId { get; init; }
        public string? SearchTerm { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class GetAllStudentsValidator : AbstractValidator<GetAllStudentsQuery>
    {
        public GetAllStudentsValidator()
        {
            RuleFor(x => x.GroupId).GreaterThan(0).When(x => x.GroupId.HasValue);
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        }
    }
}
