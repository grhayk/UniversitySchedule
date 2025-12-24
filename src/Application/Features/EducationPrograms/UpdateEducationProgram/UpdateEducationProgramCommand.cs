using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.EducationPrograms.UpdateEducationProgram
{
    public record UpdateEducationProgramCommand : IRequest<Result>
    {
        public int Id { get; init; }
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public int StructureId { get; init; }
        public int? ParentId { get; init; }
    }

    public class UpdateEducationProgramValidator : AbstractValidator<UpdateEducationProgramCommand>
    {
        public UpdateEducationProgramValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.StructureId).GreaterThan(0);
            RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId.HasValue);
        }
    }
}
