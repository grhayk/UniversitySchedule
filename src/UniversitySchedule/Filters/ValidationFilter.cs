using Application.Core;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UniversitySchedule.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. Check if the model state is invalid
            if (!context.ModelState.IsValid)
            {
                // 2. Extract all error messages into a flat list
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                // 3. Create our standardized Result failure
                var result = Result.Failure(ErrorType.Validation, "Validation failed", errors);

                // 4. Short-circuit the request. 
                // This bypasses the Controller and goes straight to your ResultFilter.
                context.Result = new BadRequestObjectResult(result);
                return;
            }

            // 5. If everything is fine, proceed to the controller action
            await next();
        }
    }
}
