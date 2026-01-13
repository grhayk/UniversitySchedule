using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Subjects.GetSubject
{
    public record GetSubjectQuery : IRequest<Result<SubjectDto>>
    {
        public int Id { get; init; }
    }

    public class GetSubjectValidator : AbstractValidator<GetSubjectQuery>
    {
        public GetSubjectValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
