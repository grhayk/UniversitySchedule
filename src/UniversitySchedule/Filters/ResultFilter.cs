using Application.Core;
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
                // Replace the result with a properly formatted HTTP response
                var apiResponse = new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    errors = result.Errors,
                    data = result is Result<object> r ? r.Data : null,
                    statusCode = result.StatusCode
                };

                context.Result = new ObjectResult(apiResponse)
                {
                    StatusCode = result.StatusCode
                };
            }

            await next();
        }
    }
}
