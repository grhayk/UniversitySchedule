using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.LecturerSubjects.RemoveLecturerSubject
{
    public record RemoveLecturerSubjectCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class RemoveLecturerSubjectValidator : AbstractValidator<RemoveLecturerSubjectCommand>
    {
        public RemoveLecturerSubjectValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
