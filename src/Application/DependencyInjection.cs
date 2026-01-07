using Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Register MediatR with Pipeline Behaviors
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);

                // Order matters - behaviors wrap each other like onion layers:
                // 1. Logging (outermost - logs everything)
                // 2. Validation (validates before handler execution)
                // 3. Exception handling (catches unhandled exceptions)
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            });

            // Register AutoMapper
            services.AddAutoMapper(assembly);

            // Register FluentValidation validators (for MediatR ValidationBehavior)
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
