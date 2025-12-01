//using Devart.Data.MySql;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using MudExtensions.Services;
using Serilog;
using TTCCashRegister.Areas.Identity;
using TTCCashRegister.Data;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.CashRegister;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;
using TTCCashRegister.Data.Export;
using TTCCashRegister.Data.Import;
using TTCCashRegister.Data.ItemDetail;
using TTCCashRegister.Data.Person;
using TTCCashRegister.Data.Source;
using TTCCashRegister.Data.SpecialItem;
using TTCCashRegister.Data.Transaction;
using TTCCashRegister.Data.TransactionDetails;
using TTCCashRegister.Data.Mapper;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMudServices();

var loggerFactory = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddConsole();
});
var logger = loggerFactory.CreateLogger<Program>();

// Add services to the container.
builder.Services.AddDbContext<CashDataContext>(options =>
{
    var configuration = builder.Configuration;
    configuration.AddUserSecrets<Program>();
    var dbPassword = Environment.GetEnvironmentVariable("DbPassword");
    if (builder.Environment.IsDevelopment())
    {
        dbPassword = builder.Configuration["DbPassword"];
    }
    var connectionString = builder.Configuration.GetConnectionString(builder.Environment.IsDevelopment() ? "DefaultConnection" : "ProductionConnection");

    if (connectionString is not null)
    {
        if (dbPassword is null)
        {
            logger.LogError($"Password not found");
            throw new Exception("Db password not found");
        }
        connectionString = connectionString.Replace("{DbPassword}", dbPassword);
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
    else
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
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
builder.Services.AddScoped<ImportCostCenterService, ImportCostCenterService>();
builder.Services.AddScoped<IImportBookingJournalService, ImportBookingJournalService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<ITransactionDetailsService, TransactionDetailsService>();
builder.Services.AddScoped<IAllocationService, AllocationService>();
builder.Services.AddScoped<IBudgetMapper, BudgetMapper>();
builder.Services.AddScoped<ICsvBudgetWriter, CsvBudgetWriter>();
builder.Services.AddScoped<IExcelBudgetWriter, ExcelBudgetWriter>();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
