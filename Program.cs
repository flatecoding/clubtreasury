//using Devart.Data.MySql;

using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using MudExtensions.Services;
using OfficeOpenXml;
using QuestPDF.Infrastructure;
using Serilog;
using TTCCashRegister;
using TTCCashRegister.Areas.Identity;
using TTCCashRegister.Data;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.CashRegister;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;
using TTCCashRegister.Data.Culture;
using TTCCashRegister.Data.Export;
using TTCCashRegister.Data.Export.Budget;
using TTCCashRegister.Data.Export.Transaction;
using TTCCashRegister.Data.Import;
using TTCCashRegister.Data.ItemDetail;
using TTCCashRegister.Data.Person;
using TTCCashRegister.Data.Source;
using TTCCashRegister.Data.SpecialItem;
using TTCCashRegister.Data.Transaction;
using TTCCashRegister.Data.TransactionDetails;
using TTCCashRegister.Data.Mapper;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddMudServices();

var loggerFactory = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddConsole();
});
var logger = loggerFactory.CreateLogger<Program>();

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
    
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(11, 0, 0))
    );
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddScoped<IItemDetailService, ItemDetailService>();
builder.Services.AddScoped<ICostCenterService, CostCenterService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ISpecialItemService, SpecialItemService>();
builder.Services.AddScoped<ICashRegisterService, CashRegisterService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IImportCostCenterService, ImportCostCenterService>();
builder.Services.AddScoped<IImportBookingJournalService, ImportBookingJournalService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<ITransactionDetailsService, TransactionDetailsService>();
builder.Services.AddScoped<IAllocationService, AllocationService>();
builder.Services.AddScoped<IBudgetMapper, BudgetMapper>();
builder.Services.AddScoped<ICsvBudgetWriter, CsvBudgetWriter>();
builder.Services.AddScoped<IExcelBudgetWriter, ExcelBudgetWriter>();
builder.Services.AddScoped<IPdfTransactionRenderer, PdfTransactionRenderer>();
builder.Services.AddScoped<ICultureService, CultureService>();
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
            logger.LogInformation("Found {Count} pending migrations: {Migrations}", pending.Count, string.Join(", ", pending));
            db.Database.Migrate();
            logger.LogInformation("Database migrated successfully");
        }
        else
        {
            logger.LogInformation("No pending migrations. Database is up-to-date.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed!");
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

// Cookie zuerst auswerten (wichtig!)
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

app.Run();
