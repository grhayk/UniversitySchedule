using Application.Core;
using Application.Models;
using MediatR;

namespace Application.Features.GroupSubjectsWithLecturer.BulkUpload
{
    public record BulkUploadGroupSubjectsWithLecturerCommand : IRequest<Result<BulkUploadResult>>
    {
        public string CsvContent { get; init; } = null!;
    }
}
