using Application.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UniversitySchedule.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // If ModelState is invalid (e.g., from FluentValidation), short-circuit
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Any() == true)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                var result = Result.Failure(400, "Validation failed", errors);

                // Return as ObjectResult so ResultFilter can format it
                context.Result = new ObjectResult(result) { StatusCode = 400 };
                return;
            }

            await next();
        }
    }
}
