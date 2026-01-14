using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Students.UpdateStudent
{
    public record UpdateStudentCommand : IRequest<Result>
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public DateTime BirthDate { get; init; }
        public int GroupId { get; init; }
    }

    public class UpdateStudentValidator : AbstractValidator<UpdateStudentCommand>
    {
        public UpdateStudentValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.BirthDate).NotEmpty().LessThan(DateTime.UtcNow);
            RuleFor(x => x.GroupId).GreaterThan(0);
        }
    }
}
