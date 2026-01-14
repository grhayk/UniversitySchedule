using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Lecturers.CreateLecturer
{
    public record CreateLecturerCommand : IRequest<Result<int>>
    {
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public DateTime BirthDate { get; init; }
        public int StructureId { get; init; }
    }

    public class CreateLecturerValidator : AbstractValidator<CreateLecturerCommand>
    {
        public CreateLecturerValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.BirthDate).NotEmpty().LessThan(DateTime.UtcNow);
            RuleFor(x => x.StructureId).GreaterThan(0);
        }
    }
}
