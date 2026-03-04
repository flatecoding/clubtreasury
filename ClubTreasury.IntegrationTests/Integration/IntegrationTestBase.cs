using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using ClubTreasury.Data;

namespace ClubTreasury.IntegrationTests.Integration;

[TestFixture]
public abstract class IntegrationTestBase
{
    private IntegrationTestWebAppFactory Factory { get; set; } = null!;
    private HttpClient Client { get; set; } = null!;
    private IServiceScope Scope { get; set; } = null!;

    private IDbContextTransaction _transaction = null!;

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
        var context = GetService<CashDataContext>();
        _transaction = await context.Database.BeginTransactionAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
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
}
