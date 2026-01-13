using Application.Core;
using Application.Models;
using FluentValidation;
using MediatR;

namespace Application.Features.Groups.BulkUpload
{
    public record BulkUploadGroupsCommand : IRequest<Result<BulkUploadResult>>
    {
        public string CsvContent { get; init; } = null!;
    }

    public class BulkUploadGroupsCommandValidator : AbstractValidator<BulkUploadGroupsCommand>
    {
        public BulkUploadGroupsCommandValidator()
        {
            RuleFor(x => x.CsvContent).NotEmpty();
        }
    }
}
