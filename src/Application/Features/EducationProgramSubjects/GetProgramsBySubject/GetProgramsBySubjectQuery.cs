using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.EducationProgramSubjects.GetProgramsBySubject
{
    public record GetProgramsBySubjectQuery : IRequest<Result<PagedResult<SubjectProgramDto>>>
    {
        public int SubjectId { get; init; }
        public bool ActiveOnly { get; init; } = true;
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class GetProgramsBySubjectValidator : AbstractValidator<GetProgramsBySubjectQuery>
    {
        public GetProgramsBySubjectValidator()
        {
            RuleFor(x => x.SubjectId).GreaterThan(0);
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        }
    }
}
