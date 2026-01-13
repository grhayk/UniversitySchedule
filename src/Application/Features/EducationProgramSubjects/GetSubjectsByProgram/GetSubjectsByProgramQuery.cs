using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.EducationProgramSubjects.GetSubjectsByProgram
{
    public record GetSubjectsByProgramQuery : IRequest<Result<PagedResult<ProgramSubjectDto>>>
    {
        public int EducationProgramId { get; init; }
        public int? SemesterId { get; init; }
        public bool ActiveOnly { get; init; } = true;
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class GetSubjectsByProgramValidator : AbstractValidator<GetSubjectsByProgramQuery>
    {
        public GetSubjectsByProgramValidator()
        {
            RuleFor(x => x.EducationProgramId).GreaterThan(0);
            RuleFor(x => x.SemesterId).GreaterThan(0).When(x => x.SemesterId.HasValue);
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        }
    }
}
