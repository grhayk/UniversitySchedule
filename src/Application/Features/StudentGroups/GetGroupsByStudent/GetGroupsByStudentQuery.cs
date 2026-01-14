using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.StudentGroups.GetGroupsByStudent
{
    public record GetGroupsByStudentQuery : IRequest<Result<List<StudentGroupDto>>>
    {
        public int StudentId { get; init; }
    }

    public class GetGroupsByStudentValidator : AbstractValidator<GetGroupsByStudentQuery>
    {
        public GetGroupsByStudentValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);
        }
    }
}
