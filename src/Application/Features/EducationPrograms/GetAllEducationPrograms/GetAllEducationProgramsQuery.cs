using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.EducationPrograms.GetAllEducationPrograms
{
    public record GetAllEducationProgramsQuery : IRequest<Result<PagedResult<EducationProgramDto>>>
    {
        public int? StructureId { get; init; }
        public int? ParentId { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class GetAllEducationProgramsValidator : AbstractValidator<GetAllEducationProgramsQuery>
    {
        public GetAllEducationProgramsValidator()
        {
            RuleFor(x => x.StructureId).GreaterThan(0).When(x => x.StructureId.HasValue);
            RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId.HasValue);
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        }
    }
}
