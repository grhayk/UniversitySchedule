using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure
{
    public class UniversityScheduleDbContextFactory : IDesignTimeDbContextFactory<UniversityScheduleDbContext>
    {
        public UniversityScheduleDbContext CreateDbContext(string[] args)
        {
            // Build config from appsettings.json in the Web project folder
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // This is the directory you run `dotnet ef` from (usually Web project)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Could not find connection string 'DefaultConnection' in appsettings.json.");

            var optionsBuilder = new DbContextOptionsBuilder<UniversityScheduleDbContext>();
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(UniversityScheduleDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure();
            });

            return new UniversityScheduleDbContext(optionsBuilder.Options);
        }
    }
}
