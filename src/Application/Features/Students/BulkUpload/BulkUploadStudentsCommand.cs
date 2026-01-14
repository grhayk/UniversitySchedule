using Application.Core;
using Application.Models;
using MediatR;

namespace Application.Features.Students.BulkUpload
{
    public record BulkUploadStudentsCommand : IRequest<Result<BulkUploadResult>>
    {
        public string CsvContent { get; init; } = null!;
    }
}
