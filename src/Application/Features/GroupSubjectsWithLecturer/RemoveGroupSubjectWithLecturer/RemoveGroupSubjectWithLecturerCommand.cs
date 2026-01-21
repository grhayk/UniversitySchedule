using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.GroupSubjectsWithLecturer.RemoveGroupSubjectWithLecturer
{
    public record RemoveGroupSubjectWithLecturerCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class RemoveGroupSubjectWithLecturerValidator : AbstractValidator<RemoveGroupSubjectWithLecturerCommand>
    {
        public RemoveGroupSubjectWithLecturerValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
