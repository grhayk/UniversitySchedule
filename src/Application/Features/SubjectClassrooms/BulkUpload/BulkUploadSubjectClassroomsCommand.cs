using Application.Core;
using Application.Models;
using MediatR;

namespace Application.Features.SubjectClassrooms.BulkUpload
{
    public record BulkUploadSubjectClassroomsCommand : IRequest<Result<BulkUploadResult>>
    {
        public string CsvContent { get; init; } = null!;
    }
}
