using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ClubTreasury.Data;

namespace ClubTreasury.Infrastructure.Startup;

public static class DatabaseInitialization
{
    public static async Task MigrateAndSeedAdminAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CashDataContext>();

        ApplyPendingMigrations(db);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        if (!userManager.Users.Any())
        {
            await SeedInitialAdminAsync(userManager, app.Environment, app.Configuration);
        }
    }

    private static void ApplyPendingMigrations(CashDataContext db)
    {
        try
        {
            var pending = db.Database.GetPendingMigrations().ToList();
            if (pending.Count != 0)
            {
                Log.Information("Found {Count} pending migrations: {Migrations}", pending.Count, string.Join(", ", pending));
                db.Database.Migrate();
                Log.Information("Database migrated successfully");
            }
            else
            {
                Log.Information("No pending migrations. Database is up-to-date.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database migration failed!");
            throw;
        }
    }

    private static async Task SeedInitialAdminAsync(
        UserManager<ApplicationUser> userManager,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        var adminUser = Environment.GetEnvironmentVariable("ADMIN_USERNAME");
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (environment.IsDevelopment())
        {
            adminUser ??= configuration["ADMIN_USERNAME"];
            adminEmail ??= configuration["ADMIN_EMAIL"];
            adminPassword ??= configuration["ADMIN_PASSWORD"];
        }

        if (string.IsNullOrWhiteSpace(adminUser)
            || string.IsNullOrWhiteSpace(adminEmail)
            || string.IsNullOrWhiteSpace(adminPassword))
        {
            Log.Warning("No users exist and ADMIN_USERNAME, ADMIN_EMAIL, or ADMIN_PASSWORD environment variables are not set. " +
                        "Set these variables to create an initial admin user.");
            return;
        }

        var user = new ApplicationUser();
        await userManager.SetUserNameAsync(user, adminUser);
        await userManager.SetEmailAsync(user, adminEmail);
        var result = await userManager.CreateAsync(user, adminPassword);

        if (result.Succeeded)
        {
            Log.Information("Initial admin user '{AdminUser}' created successfully", adminUser);
        }
        else
        {
            Log.Error("Failed to create initial admin user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}