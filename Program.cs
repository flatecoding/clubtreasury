//using Devart.Data.MySql;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using MudExtensions.Services;
using TTCCashRegister.Areas.Identity;
using TTCCashRegister.Data;
using TTCCashRegister.Data.Accounts;
using TTCCashRegister.Data.CashRegister;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;
using TTCCashRegister.Data.Export;
using TTCCashRegister.Data.Import;
using TTCCashRegister.Data.Person;
using TTCCashRegister.Data.Source;
using TTCCashRegister.Data.SpecialItem;
using TTCCashRegister.Data.SubTransaction;
using TTCCashRegister.Data.Transaction;
using TTCCashRegister.Data.UnitDetail;


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
            logger.LogInformation($"Password not found");
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
builder.Services.AddScoped<UnitDetailService>();
builder.Services.AddScoped<CostCenterService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<SpecialItemService>();
builder.Services.AddScoped<CashRegisterService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ImportCostCenterService>();
builder.Services.AddScoped<ImportBookingJournalService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<SubTransactionService>();
builder.Services.AddScoped<AccountsService>();
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

var app = builder.Build();

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
