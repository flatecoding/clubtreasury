using ClubTreasury.Data.Mapper;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Data.Export.Budget;

public class BudgetExporter(
    ITransactionService transactionService,
    IBudgetMapper budgetMapper,
    ICsvBudgetWriter csvWriter,
    IExcelBudgetWriter excelWriter,
    IResultFactory operationResultFactory,
    ILogger<BudgetExporter> logger,
    IExportPathProvider exportPathProvider
) : IBudgetExporter
{
    private readonly string _exportPath = exportPathProvider.ExportPath;

    private string GetSafeFilePath(string filename)
    {
        var sanitized = Path.GetFileName(filename);
        if (string.IsNullOrWhiteSpace(sanitized) || sanitized.Contains(".."))
            throw new ArgumentException("Invalid filename");
        return Path.Combine(_exportPath, sanitized);
    }

    public async Task<Result> ExportToCsvAsync(ExportOptions options, CancellationToken ct = default)
    {
        try
        {
            var transactions =
                await transactionService.GetTransactionsForBudgetExportAsync(options.Begin, options.End, options.CashRegisterId, ct);

            var flat = budgetMapper.BuildFlatEntries(transactions);
            var grouped = budgetMapper.BuildBudgetHierarchy(flat);

            var filePath = GetSafeFilePath(options.Filename);
            await csvWriter.WriteAsync(filePath, grouped);

            return operationResultFactory.ExportSuccessful(options.Filename);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating budget csv");
            return operationResultFactory.ExportFailed(ex.Message);
        }
    }

    public async Task<Result> ExportToExcelAsync(ExportOptions options, CancellationToken ct = default)
    {
        try
        {
            var transactions =
                await transactionService.GetTransactionsForBudgetExportAsync(options.Begin, options.End, options.CashRegisterId, ct);

            var flat = budgetMapper.BuildFlatEntries(transactions);
            var grouped = budgetMapper.BuildBudgetHierarchy(flat);

            var filePath = GetSafeFilePath(options.Filename);
            await excelWriter.WriteAsync(filePath, grouped, options.Begin, options.End);

            return operationResultFactory.ExportSuccessful(options.Filename);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Export budget to Excel failed");
            return operationResultFactory.ExportFailed(ex.Message);
        }
    }

    public async Task<byte[]> ExportToExcelBytesAsync(
        DateTime begin, DateTime end, int cashRegisterId, CancellationToken ct = default)
    {
        var filename = $"Budget_{begin:yyyyMMdd}_{end:yyyyMMdd}.xlsx";
        var request = new ExportOptions(begin, end, filename, cashRegisterId);
        var result = await ExportToExcelAsync(request, ct);

        if (result.IsFailure)
            return Array.Empty<byte>();

        var filePath = GetSafeFilePath(filename);
        return await File.ReadAllBytesAsync(filePath, ct);
    }
}