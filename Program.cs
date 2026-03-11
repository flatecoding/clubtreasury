using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Infrastructure;
using Serilog;
using ClubTreasury.Components;
using ClubTreasury.Components.Account;
using ClubTreasury.Data;
using ClubTreasury.Data.Source;
using ClubTreasury.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

QuestPDF.Settings.License = LicenseType.Community;
ExcelPackage.License.SetNonCommercialOrganization("TTC Hagen e.V.");

// Add services to the container.
builder.Services.AddDbContext<CashDataContext>(options =>
{
    var configuration = builder.Configuration;

    // Passwort aus Docker-Secret oder Dev-UserSecrets holen
    var dbPassword = Environment.GetEnvironmentVariable("DbPassword");

    if (builder.Environment.IsDevelopment())
    {
        dbPassword ??= configuration["DbPassword"];
    }

    var connectionString = configuration.GetConnectionString(
        builder.Environment.IsDevelopment() ? "DefaultConnection" : "ProductionConnection"
    );

    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Connection string missing");

    if (string.IsNullOrWhiteSpace(dbPassword))
        throw new Exception("Db password not found in environment or secrets");

    connectionString = connectionString.Replace("{DbPassword}", dbPassword);

    options.UseNpgsql(connectionString);
});
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        builder.Environment.IsDevelopment() ? "bin/keys" : "/app/keys"))
    .SetApplicationName("ClubTreasuryApp");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
}).AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;

    options.Password.RequiredLength = 10;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
})
    .AddEntityFrameworkStores<CashDataContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddApplicationServices()
                .AddValidation();

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CashDataContext>();
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

    // Seed initial admin user from environment variables if no users exist
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    if (!userManager.Users.Any())
    {
        var adminUser = Environment.GetEnvironmentVariable("ADMIN_USERNAME");
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (!string.IsNullOrWhiteSpace(adminUser) &&
            !string.IsNullOrWhiteSpace(adminEmail) &&
            !string.IsNullOrWhiteSpace(adminPassword))
        {
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
        else
        {
            Log.Warning("No users exist and ADMIN_USERNAME, ADMIN_EMAIL, or ADMIN_PASSWORD environment variables are not set. " +
                         "Set these variables to create an initial admin user.");
        }
    }
}


app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(SupportedAppCultures.English),
    SupportedCultures = SupportedAppCultures.AllCultureInfos,
    SupportedUICultures = SupportedAppCultures.AllCultureInfos
};

localizationOptions.RequestCultureProviders.Insert(
    0,
    new CookieRequestCultureProvider()
);

app.UseRequestLocalization(localizationOptions);
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Additional Identity endpoints (logout, passkey options)
app.MapAdditionalIdentityEndpoints();

app.Run();


