using Application.Core;
using Application.Models;
using FluentValidation;
using MediatR;

namespace Application.Features.EducationProgramSubjects.BulkUpload
{
    public record BulkUploadProgramSubjectsCommand : IRequest<Result<BulkUploadResult>>
    {
        public string CsvContent { get; init; } = null!;
    }

    public class BulkUploadProgramSubjectsCommandValidator : AbstractValidator<BulkUploadProgramSubjectsCommand>
    {
        public BulkUploadProgramSubjectsCommandValidator()
        {
            RuleFor(x => x.CsvContent).NotEmpty();
        }
    }
}
