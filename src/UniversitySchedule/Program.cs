using Application;
using Infrastructure;
using Infrastructure.Context;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Enrichers.Span;
using System.Threading.RateLimiting;
using UniversitySchedule.Filters;
using UniversitySchedule.Middlewares;

var builder = WebApplication.CreateBuilder(args);

//Configure swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UniversitySchedule API", Version = "v1" });
});

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan() // Adds tracing/span information
    .WriteTo.Console()
    .WriteTo.File("logs/app.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .CreateBootstrapLogger();

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
    options.Filters.Add<ResultFilter>(); // apply to all controllers
});

builder.Services.AddApplicationServices().AddInfrastructureServices();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();