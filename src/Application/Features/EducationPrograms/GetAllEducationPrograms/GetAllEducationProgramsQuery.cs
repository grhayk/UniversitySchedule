using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.EducationPrograms.GetAllEducationPrograms
{
    public record GetAllEducationProgramsQuery : IRequest<Result<List<EducationProgramDto>>>
    {
        public int? StructureId { get; init; }
        public int? ParentId { get; init; }
    }

    public class GetAllEducationProgramsValidator : AbstractValidator<GetAllEducationProgramsQuery>
    {
        public GetAllEducationProgramsValidator()
        {
            RuleFor(x => x.StructureId).GreaterThan(0).When(x => x.StructureId.HasValue);
            RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId.HasValue);
        }
    }
}
