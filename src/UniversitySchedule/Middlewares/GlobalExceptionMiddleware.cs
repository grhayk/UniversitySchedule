using System.Text.Json;

namespace UniversitySchedule.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                KeyNotFoundException => 404,
                ArgumentException => 400,
                InvalidOperationException => 400,
                _ => 500 // Unknown/unexpected errors
            };

            var message = statusCode switch
            {
                400 => "Bad Request",
                404 => "Resource not found",
                500 => "An unexpected error occurred",
                _ => "An error occurred"
            };

            // Customize message in production vs development
            var errorMessage = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                ? exception.Message
                : message;

            var response = new
            {
                success = false,
                message = errorMessage,
                errors = new[] { errorMessage },
                statusCode
            };

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
