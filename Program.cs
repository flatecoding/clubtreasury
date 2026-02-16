using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using MudExtensions.Services;
using OfficeOpenXml;
using QuestPDF.Infrastructure;
using Serilog;
using TTCCashRegister.Components;
using TTCCashRegister.Components.Account;
using TTCCashRegister.Data;
using TTCCashRegister.Data.Source;
using TTCCashRegister.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddMudServices();

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
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        builder.Environment.IsDevelopment() ? "bin/keys" : "/app/keys"))
    .SetApplicationName("ClubCashApp");

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
})
    .AddEntityFrameworkStores<CashDataContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddApplicationServices()
                .AddValidation();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;

    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 2000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Text;
});

builder.Services.AddMudBlazorDialog();
builder.Services.AddMudPopoverService();
builder.Services.AddMudExtensions();

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CashDataContext>();
    try
    {
        var pending = db.Database.GetPendingMigrations().ToList();

        if (pending.Any())
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
