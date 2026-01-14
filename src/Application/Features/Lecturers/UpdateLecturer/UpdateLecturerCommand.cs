using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Lecturers.UpdateLecturer
{
    public record UpdateLecturerCommand : IRequest<Result>
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public DateTime BirthDate { get; init; }
        public int StructureId { get; init; }
    }

    public class UpdateLecturerValidator : AbstractValidator<UpdateLecturerCommand>
    {
        public UpdateLecturerValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.BirthDate).NotEmpty().LessThan(DateTime.UtcNow);
            RuleFor(x => x.StructureId).GreaterThan(0);
        }
    }
}
