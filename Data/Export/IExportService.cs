using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Export;

public interface IExportService
{
    Task<Result> ExportTransactionsToCsvAsync(DateTime begin, DateTime end, string filename, int cashRegisterId, CancellationToken ct = default);
    Task<Result> ExportTransactionsToPdfAsync(DateTime begin, DateTime end, string filename, int cashRegisterId, string cashRegisterName, CancellationToken cancellationToken);
    Task<Result> ExportBudgetToCsvAsync(DateTime begin, DateTime end, string filename, int cashRegisterId, CancellationToken ct = default);
    Task<Result> ExportBudgetToExcelAsync(DateTime begin, DateTime end, string filename, int cashRegisterId, CancellationToken ct = default);
    Task<byte[]> ExportBudgetToExcelBytesAsync(DateTime begin, DateTime end, int cashRegisterId, CancellationToken ct = default);
}