using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.LecturerSubjects.GetLecturersBySubject
{
    public record GetLecturersBySubjectQuery : IRequest<Result<List<SubjectLecturerListDto>>>
    {
        public int SubjectId { get; init; }
    }

    public class GetLecturersBySubjectValidator : AbstractValidator<GetLecturersBySubjectQuery>
    {
        public GetLecturersBySubjectValidator()
        {
            RuleFor(x => x.SubjectId).GreaterThan(0);
        }
    }
}
