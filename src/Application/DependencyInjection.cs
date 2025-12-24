using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // Register AutoMapper
            services.AddAutoMapper(assembly);

            // Register FluentValidation
            services.AddFluentValidationAutoValidation(); // auto-validate on model binding
            services.AddFluentValidationClientsideAdapters(); // Optional (for Blazor/Razor)
            services.AddValidatorsFromAssembly(assembly);


            return services;
        }
    }
}
