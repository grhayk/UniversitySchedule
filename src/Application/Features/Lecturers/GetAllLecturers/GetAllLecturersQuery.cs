using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Lecturers.GetAllLecturers
{
    public record GetAllLecturersQuery : IRequest<Result<PagedResult<LecturerListDto>>>
    {
        public int? StructureId { get; init; }
        public string? SearchTerm { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class GetAllLecturersValidator : AbstractValidator<GetAllLecturersQuery>
    {
        public GetAllLecturersValidator()
        {
            RuleFor(x => x.StructureId).GreaterThan(0).When(x => x.StructureId.HasValue);
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        }
    }
}
