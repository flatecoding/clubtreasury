using ClubTreasury.Data.Export.Budget;
using ClubTreasury.Data.Export.Transaction;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Export;

public class ExportService(
    ITransactionExporter transactionExporter,
    IBudgetExporter budgetExporter
) : IExportService
{
    public Task<Result> ExportTransactionsToCsvAsync(ExportOptions options, CancellationToken ct = default)
        => transactionExporter.ExportToCsvAsync(options, ct);

    public Task<Result> ExportTransactionsToPdfAsync(PdfExportOptions options, CancellationToken ct = default)
        => transactionExporter.ExportToPdfAsync(options, ct);

    public Task<Result> ExportBalanceSheetToCsvAsync(ExportOptions options, CancellationToken ct = default)
        => budgetExporter.ExportBalanceSheetToCsvAsync(options, ct);

    public Task<Result> ExportBalanceSheetToExcelAsync(ExportOptions options, CancellationToken ct = default)
        => budgetExporter.ExportBalanceSheetToExcelAsync(options, ct);

    public Task<Result> ExportBudgetPlanToExcelAsync(ExportOptions options, CancellationToken ct = default)
        => budgetExporter.ExportBudgetPlanToExcelAsync(options, ct);

    public Task<byte[]> ExportBudgetToExcelBytesAsync(DateTime begin, DateTime end, int cashRegisterId, CancellationToken ct = default)
        => budgetExporter.ExportToExcelBytesAsync(begin, end, cashRegisterId, ct);
}