using Application.Core;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.GroupSubjectsWithLecturer.GetByLecturerSubject
{
    public record GetByLecturerSubjectQuery : IRequest<Result<List<LecturerSubjectGroupListDto>>>
    {
        public int LecturerSubjectId { get; init; }
        public LessonType? LessonType { get; init; }
    }

    public class GetByLecturerSubjectValidator : AbstractValidator<GetByLecturerSubjectQuery>
    {
        public GetByLecturerSubjectValidator()
        {
            RuleFor(x => x.LecturerSubjectId).GreaterThan(0);
            RuleFor(x => x.LessonType).IsInEnum().When(x => x.LessonType.HasValue);
        }
    }
}
