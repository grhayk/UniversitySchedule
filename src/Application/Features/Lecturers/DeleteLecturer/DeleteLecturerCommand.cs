using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Lecturers.DeleteLecturer
{
    public record DeleteLecturerCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class DeleteLecturerValidator : AbstractValidator<DeleteLecturerCommand>
    {
        public DeleteLecturerValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
