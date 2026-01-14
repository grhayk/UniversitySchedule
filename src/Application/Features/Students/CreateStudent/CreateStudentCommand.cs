using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Students.CreateStudent
{
    public record CreateStudentCommand : IRequest<Result<int>>
    {
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public DateTime BirthDate { get; init; }
        public int GroupId { get; init; }
    }

    public class CreateStudentValidator : AbstractValidator<CreateStudentCommand>
    {
        public CreateStudentValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.BirthDate).NotEmpty().LessThan(DateTime.UtcNow);
            RuleFor(x => x.GroupId).GreaterThan(0);
        }
    }
}
