using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors
{
    /// <summary>
    /// Catches and logs unhandled exceptions from MediatR handlers.
    /// Exceptions are re-thrown to be handled by GlobalExceptionMiddleware.
    /// </summary>
    public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

        public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;
                _logger.LogError(ex, "Exception in {RequestName}: {ExceptionType} - {Message}",
                    requestName, ex.GetType().Name, ex.Message);

                // Re-throw to let GlobalExceptionMiddleware handle it
                throw;
            }
        }
    }
}
