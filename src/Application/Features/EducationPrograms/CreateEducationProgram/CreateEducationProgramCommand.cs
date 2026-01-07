using Application.Core;
using FluentValidation;
using MediatR;

namespace Application.Features.EducationPrograms.CreateEducationProgram
{
    public record CreateEducationProgramCommand : IRequest<Result<int>>
    {
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public int StructureId { get; init; }
        public int? ParentId { get; init; }
    }

    public class CreateEducationProgramValidator : AbstractValidator<CreateEducationProgramCommand>
    {
        public CreateEducationProgramValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.StructureId).GreaterThan(0);
            RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId.HasValue);
        }
    }
}
