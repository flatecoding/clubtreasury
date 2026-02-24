using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Export;

public interface IExportService
{
    Task<IOperationResult> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename, int cashRegisterId);
    Task<IOperationResult> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename, int cashRegisterId, string cashRegisterName, CancellationToken cancellationToken);
    Task<IOperationResult> ExportBudgetToCsv(DateTime begin, DateTime end, string filename, int cashRegisterId);
    Task<IOperationResult> ExportBudgetToExcel(DateTime begin, DateTime end, string filename, int cashRegisterId);
    Task<byte[]> ExportBudgetToExcelBytes(DateTime begin, DateTime end, int cashRegisterId);
}