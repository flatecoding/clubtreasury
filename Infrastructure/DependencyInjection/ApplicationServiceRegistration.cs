using MudBlazor;
using MudBlazor.Services;
using MudExtensions.Services;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.Culture;
using ClubTreasury.Data.Export;
using ClubTreasury.Data.Export.Budget;
using ClubTreasury.Data.Export.Transaction;
using ClubTreasury.Data.Import;
using ClubTreasury.Data.ItemDetail;
using ClubTreasury.Data.Mapper;
using ClubTreasury.Data.Notification;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.Person;
using ClubTreasury.Data.SpecialItem;
using ClubTreasury.Data.ThemeSetting;
using ClubTreasury.Data.Transaction;
using ClubTreasury.Data.TransactionDetails;
using ClubTreasury.Infrastructure.Localization;

namespace ClubTreasury.Infrastructure.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IItemDetailService, ItemDetailService>();
        services.AddScoped<ICostCenterService, CostCenterService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ISpecialItemService, SpecialItemService>();
        services.AddScoped<ICashRegisterService, CashRegisterService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IImportCostCenterService, ImportCostCenterService>();
        services.AddScoped<IImportBookingJournalService, ImportBookingJournalService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<ITransactionDetailsService, TransactionDetailsService>();
        services.AddScoped<IAllocationService, AllocationService>();
        services.AddScoped<IBudgetMapper, BudgetMapper>();
        services.AddScoped<ICsvBudgetWriter, CsvBudgetWriter>();
        services.AddScoped<IExcelBudgetWriter, ExcelBudgetWriter>();
        services.AddScoped<IPdfTransactionRenderer, PdfTransactionRenderer>();
        services.AddScoped<ICultureService, CultureService>();
        services.AddScoped<IOperationResultFactory, OperationResultFactory>();
        services.AddSingleton<IExportPathProvider, ExportPathProvider>();
        services.AddHttpContextAccessor();
        services.AddScoped<UserPrefService>(sp =>
        {
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            var logger = sp.GetRequiredService<ILogger<UserPrefService>>();
            var context = httpContextAccessor.HttpContext;
            var raw = context?.Request.Cookies[UserPrefService.StorageKey];
            var userPrefDataString = raw != null ? Uri.UnescapeDataString(raw) : string.Empty;
            return new UserPrefService(userPrefDataString, logger);
        });
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddMudServices(config =>
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
        services.AddLocalizationInterceptor<MudBlazorLocalizationInterceptor>();
        services.AddMudBlazorDialog();
        services.AddMudPopoverService();
        services.AddMudExtensions();

        return services;
    }
}