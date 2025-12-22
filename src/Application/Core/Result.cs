namespace Application.Core
{
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public List<string> Errors { get; private set; } = new();
        public int StatusCode { get; private set; }

        protected Result() { }

        public static Result Success(string message = "Success") =>
            new() { IsSuccess = true, Message = message, StatusCode = 200 };

        public static Result<T> Success<T>(T data, string message = "Success") =>
            new() { IsSuccess = true, Data = data, Message = message, StatusCode = 200 };

        public static Result Failure(int statusCode, string message, IEnumerable<string>? errors = null) =>
            new() { IsSuccess = false, StatusCode = statusCode, Message = message, Errors = errors?.ToList() ?? new() };

        public static Result<T> Failure<T>(int statusCode, string message, IEnumerable<string>? errors = null) =>
            new() { IsSuccess = false, StatusCode = statusCode, Message = message, Errors = errors?.ToList() ?? new() };
    }

    public class Result<T> : Result
    {
        public T? Data { get; set; }
    }
}
