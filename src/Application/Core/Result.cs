using Domain.Enums;
using System.Text.Json.Serialization;

namespace Application.Core
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string Message { get; }
        public List<string> Errors { get; }

        // Internal - used for status code mapping, not exposed in JSON
        [JsonIgnore]
        public ErrorType ErrorType { get; }

        protected Result(bool isSuccess, string message, ErrorType errorType, List<string>? errors = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            ErrorType = errorType;
            Errors = errors ?? new();
        }

        // Helper to allow the filter to access Data without knowing the T type
        public virtual object? GetValue() => null;

        public static Result Success(string message = "Success")
            => new(true, message, ErrorType.None);

        public static Result<T> Success<T>(T data, string message = "Success")
            => new(data, true, message, ErrorType.None);

        public static Result Failure(ErrorType errorType, string message, List<string>? errors = null)
            => new(false, message, errorType, errors);

        public static Result<T> Failure<T>(ErrorType errorType, string message, List<string>? errors = null)
            => new(default, false, message, errorType, errors);
    }

    public class Result<T> : Result
    {
        public T? Data { get; }
        public override object? GetValue() => Data;

        // Internal constructor to ensure it's created via the static methods
        internal Result(T? data, bool isSuccess, string message, ErrorType errorType, List<string>? errors = null)
            : base(isSuccess, message, errorType, errors)
        {
            Data = data;
        }
    }
}
