using Application.Core;
using Application.Models;
using FluentValidation;
using MediatR;

namespace Application.Features.Subjects.BulkUpload
{
    public record BulkUploadSubjectsCommand : IRequest<Result<BulkUploadResult>>
    {
        public string CsvContent { get; init; } = null!;
    }

    public class BulkUploadSubjectsCommandValidator : AbstractValidator<BulkUploadSubjectsCommand>
    {
        public BulkUploadSubjectsCommandValidator()
        {
            RuleFor(x => x.CsvContent).NotEmpty();
        }
    }
}
