using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Export;

public interface IExportService
{
    Task<Result> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename, int cashRegisterId, CancellationToken ct = default);
    Task<Result> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename, int cashRegisterId, string cashRegisterName, CancellationToken cancellationToken);
    Task<Result> ExportBudgetToCsv(DateTime begin, DateTime end, string filename, int cashRegisterId, CancellationToken ct = default);
    Task<Result> ExportBudgetToExcel(DateTime begin, DateTime end, string filename, int cashRegisterId, CancellationToken ct = default);
    Task<byte[]> ExportBudgetToExcelBytes(DateTime begin, DateTime end, int cashRegisterId, CancellationToken ct = default);
}