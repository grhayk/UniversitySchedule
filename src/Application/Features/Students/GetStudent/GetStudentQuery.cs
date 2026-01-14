using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.Students.GetStudent
{
    public record GetStudentQuery : IRequest<Result<StudentDto>>
    {
        public int Id { get; init; }
    }

    public class GetStudentValidator : AbstractValidator<GetStudentQuery>
    {
        public GetStudentValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
