using Application.Core;
using Application.Models;
using MediatR;

namespace Application.Features.LecturerSubjects.BulkUpload
{
    public record BulkUploadLecturerSubjectsCommand : IRequest<Result<BulkUploadResult>>
    {
        public string CsvContent { get; init; } = null!;
    }
}
