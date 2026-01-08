using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Classrooms.GetAllClassrooms
{
    public record GetAllClassroomsQuery : IRequest<Result<PagedResult<ClassroomDto>>>
    {
        public int? StructureId { get; init; }
        public ClassroomType? ClassroomType { get; init; }
        public int? MinSeatCapacity { get; init; }
        public int? MaxSeatCapacity { get; init; }
        public bool? HasComputer { get; init; }
        public bool? HasProjector { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class GetAllClassroomsValidator : AbstractValidator<GetAllClassroomsQuery>
    {
        public GetAllClassroomsValidator()
        {
            RuleFor(x => x.StructureId).GreaterThan(0).When(x => x.StructureId.HasValue);
            RuleFor(x => x.ClassroomType).IsInEnum().When(x => x.ClassroomType.HasValue);
            RuleFor(x => x.MinSeatCapacity).GreaterThan(0).When(x => x.MinSeatCapacity.HasValue);
            RuleFor(x => x.MaxSeatCapacity).GreaterThan(0).When(x => x.MaxSeatCapacity.HasValue);
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        }
    }
}
