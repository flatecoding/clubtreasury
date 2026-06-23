using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Export;

public interface IExportService
{
    Task<Result> ExportTransactionsToCsvAsync(ExportOptions options, CancellationToken ct = default);
    Task<Result> ExportTransactionsToPdfAsync(PdfExportOptions options, CancellationToken ct = default);
    Task<Result> ExportBalanceSheetToCsvAsync(ExportOptions options, CancellationToken ct = default);
    Task<Result> ExportBalanceSheetToExcelAsync(ExportOptions options, CancellationToken ct = default);
    Task<Result> ExportBudgetPlanToExcelAsync(ExportOptions options, CancellationToken ct = default);
    Task<byte[]> ExportBudgetToExcelBytesAsync(DateTime begin, DateTime end, int cashRegisterId, CancellationToken ct = default);
}