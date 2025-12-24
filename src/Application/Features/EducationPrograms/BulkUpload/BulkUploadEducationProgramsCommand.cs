using Application.Core;
using Application.Models;
using FluentValidation;
using MediatR;

namespace Application.Features.EducationPrograms.BulkUpload
{
    public record BulkUploadEducationProgramsCommand : IRequest<Result<BulkUploadResult>>
    {
        public string CsvContent { get; init; } = null!;
    }

    public class BulkUploadEducationProgramsCommandValidator : AbstractValidator<BulkUploadEducationProgramsCommand>
    {
        public BulkUploadEducationProgramsCommandValidator()
        {
            RuleFor(x => x.CsvContent).NotEmpty();
        }
    }
}
