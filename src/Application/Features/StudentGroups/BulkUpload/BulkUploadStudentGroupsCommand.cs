using Application.Core;
using Application.Models;
using MediatR;

namespace Application.Features.StudentGroups.BulkUpload
{
    public record BulkUploadStudentGroupsCommand : IRequest<Result<BulkUploadResult>>
    {
        public string CsvContent { get; init; } = null!;
    }
}
