using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Lecturers.GetLecturer
{
    public record GetLecturerQuery : IRequest<Result<LecturerDto>>
    {
        public int Id { get; init; }
    }

    public class GetLecturerValidator : AbstractValidator<GetLecturerQuery>
    {
        public GetLecturerValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
