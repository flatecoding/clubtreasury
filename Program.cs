using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using MudExtensions.Services;
using OfficeOpenXml;
using QuestPDF.Infrastructure;
using Serilog;
using TTCCashRegister.Areas.Identity;
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
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
}).AddEntityFrameworkStores<CashDataContext>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
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
app.UseStaticFiles();
app.UseRouting();

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

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Passkey API endpoints
var accountGroup = app.MapGroup("/Account");

accountGroup.MapPost("/PasskeyCreationOptions", async (
    HttpContext context,
    [FromServices] UserManager<ApplicationUser> userManager,
    [FromServices] SignInManager<ApplicationUser> signInManager,
    [FromServices] IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);

    var user = await userManager.GetUserAsync(context.User);
    if (user is null)
    {
        return Results.NotFound($"Unable to load user with ID '{userManager.GetUserId(context.User)}'.");
    }

    var userId = await userManager.GetUserIdAsync(user);
    var userName = await userManager.GetUserNameAsync(user) ?? "User";
    var optionsJson = await signInManager.MakePasskeyCreationOptionsAsync(new()
    {
        Id = userId,
        Name = userName,
        DisplayName = userName
    });
    return TypedResults.Content(optionsJson, contentType: "application/json");
}).RequireAuthorization();

accountGroup.MapPost("/PasskeyRequestOptions", async (
    HttpContext context,
    [FromServices] UserManager<ApplicationUser> userManager,
    [FromServices] SignInManager<ApplicationUser> signInManager,
    [FromServices] IAntiforgery antiforgery,
    [FromQuery] string? username) =>
{
    await antiforgery.ValidateRequestAsync(context);

    var user = string.IsNullOrEmpty(username) ? null : await userManager.FindByNameAsync(username);
    var optionsJson = await signInManager.MakePasskeyRequestOptionsAsync(user);
    return TypedResults.Content(optionsJson, contentType: "application/json");
});

app.Run();
