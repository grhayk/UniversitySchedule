using Application.Core;
using Application.Models;
using MediatR;

namespace Application.Features.Schedules.BulkUpload
{
    public record BulkUploadScheduleCommand : IRequest<Result<BulkUploadResult>>
    {
        public string CsvContent { get; init; } = null!;
    }
}
