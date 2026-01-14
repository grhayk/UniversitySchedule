using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Students.DeleteStudent
{
    public record DeleteStudentCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class DeleteStudentValidator : AbstractValidator<DeleteStudentCommand>
    {
        public DeleteStudentValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
