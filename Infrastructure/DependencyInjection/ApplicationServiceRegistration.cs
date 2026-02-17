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
using TTCCashRegister.Data.Mapper;
using TTCCashRegister.Data.Notification;
using TTCCashRegister.Data.OperationResult;
using TTCCashRegister.Data.Person;
using TTCCashRegister.Data.SpecialItem;
using TTCCashRegister.Data.ThemeSetting;
using TTCCashRegister.Data.Transaction;
using TTCCashRegister.Data.TransactionDetails;

namespace TTCCashRegister.Infrastructure.DependencyInjection;

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
        
        return services;
    }
}