using Domain.Enums;
using System.Net;
using System.Text.Json;

namespace UniversitySchedule.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate _next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
        {
            this._next = _next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // 1. Map the exception to our ErrorType and Status Code
            var (errorType, statusCode) = exception switch
            {
                UnauthorizedAccessException => (ErrorType.Unauthorized, (int)HttpStatusCode.Unauthorized),
                KeyNotFoundException => (ErrorType.NotFound, (int)HttpStatusCode.NotFound),
                ArgumentException => (ErrorType.Validation, (int)HttpStatusCode.BadRequest),
                _ => (ErrorType.Failure, (int)HttpStatusCode.InternalServerError)
            };

            // 2. Determine the message (hide details in Production)
            string message = _env.IsDevelopment()
                ? exception.Message
                : "An unexpected error occurred on the server.";

            // 3. Use the same structure as our ResultFilter
            // We manually create the payload to ensure it matches the "shaped" response
            var response = new
            {
                IsSuccess = false,
                Message = message,
                Errors = _env.IsDevelopment() ? new List<string> { exception.StackTrace ?? "" } : new List<string> { "Internal Server Error" },
                Data = (object?)null
            };

            context.Response.StatusCode = statusCode;

            // 4. Serialize with CamelCase to match standard JSON conventions
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
