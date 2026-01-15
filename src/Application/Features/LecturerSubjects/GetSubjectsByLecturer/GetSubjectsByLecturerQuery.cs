using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.LecturerSubjects.GetSubjectsByLecturer
{
    public record GetSubjectsByLecturerQuery : IRequest<Result<List<LecturerSubjectListDto>>>
    {
        public int LecturerId { get; init; }
    }

    public class GetSubjectsByLecturerValidator : AbstractValidator<GetSubjectsByLecturerQuery>
    {
        public GetSubjectsByLecturerValidator()
        {
            RuleFor(x => x.LecturerId).GreaterThan(0);
        }
    }
}
