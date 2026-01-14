using Application.Core;
using Application.Models;
using MediatR;

namespace Application.Features.Lecturers.BulkUpload
{
    public record BulkUploadLecturersCommand : IRequest<Result<BulkUploadResult>>
    {
        public string CsvContent { get; init; } = null!;
    }
}
