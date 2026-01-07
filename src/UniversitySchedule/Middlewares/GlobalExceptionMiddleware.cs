using FluentValidation;
using System.Net;
using System.Text.Json;

namespace UniversitySchedule.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
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
                _logger.LogError(ex, "Unhandled exception reached GlobalExceptionMiddleware: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Check if response has already started
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started, cannot handle exception");
                return;
            }

            context.Response.ContentType = "application/json";

            // Map exception to HTTP status code
            var statusCode = exception switch
            {
                ValidationException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                KeyNotFoundException => HttpStatusCode.NotFound,
                ArgumentException => HttpStatusCode.BadRequest,
                InvalidOperationException => HttpStatusCode.Conflict,
                _ => HttpStatusCode.InternalServerError
            };

            // Determine the message and errors
            string message;
            List<string> errors;

            if (exception is ValidationException validationException)
            {
                // Handle FluentValidation exceptions specially
                message = "Validation failed";
                errors = validationException.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
            }
            else
            {
                // Handle other exceptions
                message = _env.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred on the server.";

                errors = new List<string>();
                if (_env.IsDevelopment())
                {
                    errors.Add(exception.Message);
                    if (exception.InnerException != null)
                    {
                        errors.Add($"Inner: {exception.InnerException.Message}");
                    }
                }
                else
                {
                    errors.Add("An error occurred while processing your request");
                }
            }

            // Use the same structure as ResultFilter
            var response = new
            {
                IsSuccess = false,
                Message = message,
                Errors = errors,
                Data = (object?)null
            };

            context.Response.StatusCode = (int)statusCode;

            // Serialize with CamelCase to match ASP.NET conventions
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
