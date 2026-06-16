using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ClubTreasury.Data;

namespace ClubTreasury.IntegrationTests.Integration;

[TestFixture]
public abstract class IntegrationTestBase
{
    private IntegrationTestWebAppFactory Factory { get; set; } = null!;
    private HttpClient Client { get; set; } = null!;
    private IServiceScope Scope { get; set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        Factory = new IntegrationTestWebAppFactory();
        await Factory.InitializeAsync();
        Client = Factory.CreateClient();
    }

    [SetUp]
    public async Task SetUp()
    {
        Scope = Factory.Services.CreateScope();

        // Services create their own contexts via the factory and commit independently, so each test
        // starts from a clean database rather than relying on a shared transaction rollback.
        await ResetDatabaseAsync();
    }

    [TearDown]
    public void TearDown()
    {
        Scope.Dispose();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }

    protected T GetService<T>() where T : notnull
        => Scope.ServiceProvider.GetRequiredService<T>();

    protected CashDataContext GetDbContext()
        => GetService<CashDataContext>();

    private async Task ResetDatabaseAsync()
    {
        var context = GetDbContext();

        var tableNames = context.Model.GetEntityTypes()
            .Select(entityType => entityType.GetTableName())
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .Select(name => $"\"{name}\"");

        await context.Database.ExecuteSqlRawAsync(
            $"TRUNCATE {string.Join(", ", tableNames)} RESTART IDENTITY CASCADE");
    }
}
