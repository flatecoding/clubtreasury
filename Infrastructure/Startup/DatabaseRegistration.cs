using Microsoft.EntityFrameworkCore;
using ClubTreasury.Data;

namespace ClubTreasury.Infrastructure.Startup;

public static class DatabaseRegistration
{
    public static IServiceCollection AddCashDataContext(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        services.AddDbContextFactory<CashDataContext>(options =>
        {
            var connectionString = BuildConnectionString(environment, configuration);
            options.UseNpgsql(connectionString);
        });

        // ASP.NET Core Identity resolves CashDataContext directly, so it still needs a scoped
        // registration. Bridge it through the factory so both share the same configuration.
        services.AddScoped<CashDataContext>(serviceProvider =>
            serviceProvider.GetRequiredService<IDbContextFactory<CashDataContext>>().CreateDbContext());

        return services;
    }

    private static string BuildConnectionString(IHostEnvironment environment, IConfiguration configuration)
    {
        var dbPassword = Environment.GetEnvironmentVariable("DbPassword");
        var dbUser = Environment.GetEnvironmentVariable("DbUser");
        var dbName = Environment.GetEnvironmentVariable("DbName");

        if (environment.IsDevelopment())
        {
            dbPassword ??= configuration["DbPassword"];
            dbUser ??= configuration["DbUser"];
            dbName ??= configuration["DbName"];
        }

        var connectionString = configuration.GetConnectionString(
            environment.IsDevelopment() ? "DefaultConnection" : "ProductionConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string missing");
        if (string.IsNullOrWhiteSpace(dbPassword))
            throw new InvalidOperationException("Db password not found in environment or secrets");
        if (string.IsNullOrWhiteSpace(dbUser))
            throw new InvalidOperationException("Db user not found in environment or secrets");
        if (string.IsNullOrWhiteSpace(dbName))
            throw new InvalidOperationException("Db name not found in environment or secrets");

        return connectionString
            .Replace("{DbName}", dbName)
            .Replace("{DbUser}", dbUser)
            .Replace("{DbPassword}", dbPassword);
    }
}