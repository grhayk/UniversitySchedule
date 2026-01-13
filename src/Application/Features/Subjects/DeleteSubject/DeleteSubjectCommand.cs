using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Subjects.DeleteSubject
{
    public record DeleteSubjectCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class DeleteSubjectValidator : AbstractValidator<DeleteSubjectCommand>
    {
        public DeleteSubjectValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
