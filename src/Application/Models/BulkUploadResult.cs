namespace Application.Models
{
    public record BulkUploadResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<BulkUploadError> Errors { get; init; } = new();
    }

    public record BulkUploadError
    {
        public int RowNumber { get; init; }
        public string Message { get; init; } = null!;
        public string? Code { get; init; }
    }
}
