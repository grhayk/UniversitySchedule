using Application.Interfaces;
using Domain.Interfaces;
using Infrastructure.Context;
using Infrastructure.Implementations;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddSingleton<IClock, SystemClock>();
            services.AddSingleton<IScheduleGenerator, ScheduleGenerator>();
            services.AddScoped<IDbContext>(provider => provider.GetRequiredService<UniversityScheduleDbContext>());

            return services;
        }
    }
}
