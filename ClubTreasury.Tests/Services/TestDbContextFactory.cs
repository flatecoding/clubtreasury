using Microsoft.EntityFrameworkCore;
using ClubTreasury.Data;

namespace ClubTreasury.Tests.Services;

/// <summary>
/// Creates <see cref="CashDataContext"/> instances over the supplied in-memory options, mirroring the
/// per-operation factory the services use in production. When <paramref name="disposed"/> is true, every
/// created context is pre-disposed so service catch blocks can be exercised (simulating a database failure).
/// </summary>
public sealed class TestDbContextFactory(DbContextOptions<CashDataContext> options, bool disposed = false)
    : IDbContextFactory<CashDataContext>
{
    public CashDataContext CreateDbContext()
    {
        var context = new CashDataContext(options);
        if (disposed)
        {
            context.Dispose();
        }

        return context;
    }
}