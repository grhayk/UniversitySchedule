using Application.Core;
using Domain.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Context;
using Infrastructure.Implementations;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Enrichers.Span;
using System.Threading.RateLimiting;
using UniversitySchedule.Filters;
using UniversitySchedule.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan() // Adds tracing/span information
    .WriteTo.Console()
    .WriteTo.File("logs/app.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .CreateBootstrapLogger(); // Allows logging during startup

builder.Host.UseSerilog(); // Plug into ASP.NET Core logging


// Add services to the container.
builder.Services.AddDbContext<UniversityScheduleDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);

            // Tell EF Core where the migrations are
            sqlOptions.MigrationsAssembly(typeof(UniversityScheduleDbContext).Assembly.FullName);
        }));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<UniversityScheduleDbContext>()
    .AddCheck("Self", () => HealthCheckResult.Healthy("App is running")); // Basic self-check

// Add services
builder.Services.AddRateLimiter(options =>
{
    // Global: max 100 requests per minute per IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<ResultFilter>(); // apply to all controllers
});

builder.Services.AddFluentValidationAutoValidation(); // auto-validate on model binding
builder.Services.AddFluentValidationClientsideAdapters(); // Optional (for Blazor/Razor)
builder.Services.AddValidatorsFromAssembly(typeof(Result).Assembly);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>(); // So can catch exceptions from all downstream middlewares
app.UseHttpsRedirection(); // Redirect HTTP to HTTPS before any logic. Safe before routing.

app.UseRouting(); // Resolves which endpoint will handle the request.

app.UseSerilogRequestLogging(); // Should be after routing to capture endpoint info

app.UseRateLimiter(); // Needs routing to resolve endpoint
app.UseAuthorization();

app.MapGet("/throw", (hc) => throw new Exception("Boom!"));
app.MapHealthChecks("/health");
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.MapControllers();

app.Run();