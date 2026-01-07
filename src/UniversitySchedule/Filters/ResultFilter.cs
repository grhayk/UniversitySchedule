using Application.Core;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UniversitySchedule.Filters
{
    public class ResultFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value is Result result)
            {
                // 1. Determine the correct HTTP Status Code based on ErrorType
                int statusCode = result.ErrorType switch
                {
                    ErrorType.None => StatusCodes.Status200OK,
                    ErrorType.Validation => StatusCodes.Status400BadRequest,
                    ErrorType.NotFound => StatusCodes.Status404NotFound,
                    ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                    ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                    ErrorType.Conflict => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status500InternalServerError
                };

                // 2. We return the Result object directly. 
                // ASP.NET will serialize IsSuccess, Message, Errors, and Data (if present).
                // We use the GetValue() helper to ensure the "Data" field is populated if it's a Result<T>.
                var responsePayload = new
                {
                    result.IsSuccess,
                    result.Message,
                    result.Errors,
                    Data = result.GetValue()
                };

                context.Result = new ObjectResult(responsePayload)
                {
                    StatusCode = statusCode
                };
            }

            await next();
        }
    }
}
