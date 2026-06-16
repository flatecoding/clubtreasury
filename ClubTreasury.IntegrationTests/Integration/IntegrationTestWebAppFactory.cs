using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using ClubTreasury.Data;

namespace ClubTreasury.IntegrationTests.Integration;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("ClubCash")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                ["ConnectionStrings:ProductionConnection"] = _dbContainer.GetConnectionString(),
                ["DbName"] = "notused",
                ["DbUser"] = "notused",
                ["DbPassword"] = "notused"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace the application's context factory (which targets the production connection)
            // with one bound to the test container, keeping the scoped bridge Identity relies on.
            var descriptors = services
                .Where(d => d.ServiceType == typeof(IDbContextFactory<CashDataContext>)
                            || d.ServiceType == typeof(DbContextOptions<CashDataContext>)
                            || d.ServiceType == typeof(CashDataContext))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContextFactory<CashDataContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
            services.AddScoped<CashDataContext>(serviceProvider =>
                serviceProvider.GetRequiredService<IDbContextFactory<CashDataContext>>().CreateDbContext());
        });

        builder.UseEnvironment("Development");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await base.DisposeAsync();
    }
}
