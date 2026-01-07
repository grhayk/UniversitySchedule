using FluentValidation;
using MediatR;

namespace Application.Behaviors
{
    /// <summary>
    /// Intercepts MediatR requests and runs FluentValidation validators.
    /// Throws ValidationException if validation fails (caught by GlobalExceptionMiddleware).
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // If no validators registered, skip
            if (!_validators.Any())
            {
                return await next();
            }

            // Run all validators
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            // Collect all validation failures
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            // If validation failed, throw ValidationException
            // GlobalExceptionMiddleware will catch it and return proper JSON response
            if (failures.Any())
            {
                throw new ValidationException(failures);
            }

            // Validation passed, continue to handler
            return await next();
        }
    }
}
