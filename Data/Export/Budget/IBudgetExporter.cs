using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Export.Budget;

public interface IBudgetExporter
{
    Task<Result> ExportToCsvAsync(ExportOptions options, CancellationToken ct = default);
    Task<Result> ExportToExcelAsync(ExportOptions options, CancellationToken ct = default);
    Task<byte[]> ExportToExcelBytesAsync(DateTime begin, DateTime end, int cashRegisterId, CancellationToken ct = default);
}