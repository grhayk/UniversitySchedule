using Application.Core;
using Application.Models;
using FluentValidation;
using MediatR;

namespace Application.Features.Classrooms.BulkUpload
{
    public record BulkUploadClassroomsCommand : IRequest<Result<BulkUploadResult>>
    {
        public string CsvContent { get; init; } = null!;
    }

    public class BulkUploadClassroomsCommandValidator : AbstractValidator<BulkUploadClassroomsCommand>
    {
        public BulkUploadClassroomsCommandValidator()
        {
            RuleFor(x => x.CsvContent).NotEmpty();
        }
    }
}
