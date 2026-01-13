using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Subjects.GetAllSubjects
{
    public record GetAllSubjectsQuery : IRequest<Result<PagedResult<SubjectDto>>>
    {
        public int? StructureId { get; init; }
        public int? SemesterId { get; init; }
        public string? SearchTerm { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class GetAllSubjectsValidator : AbstractValidator<GetAllSubjectsQuery>
    {
        public GetAllSubjectsValidator()
        {
            RuleFor(x => x.StructureId).GreaterThan(0).When(x => x.StructureId.HasValue);
            RuleFor(x => x.SemesterId).GreaterThan(0).When(x => x.SemesterId.HasValue);
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        }
    }
}
