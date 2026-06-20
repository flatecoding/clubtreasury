using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Export.Budget;

public interface IBudgetExporter
{
    Task<Result> ExportBalanceSheetToCsvAsync(ExportOptions options, CancellationToken ct = default);
    Task<Result> ExportBalanceSheetToExcelAsync(ExportOptions options, CancellationToken ct = default);
    Task<Result> ExportBudgetPlanToExcelAsync(ExportOptions options, CancellationToken ct = default);
    Task<byte[]> ExportToExcelBytesAsync(DateTime begin, DateTime end, int cashRegisterId, CancellationToken ct = default);
}