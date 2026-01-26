using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Export;

public interface IExportService
{
    Task<IOperationResult> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename);
    Task<IOperationResult> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename, CancellationToken cancellationToken);
    Task<IOperationResult> ExportBudgetToCsv(DateTime begin, DateTime end, string filename);
    Task<IOperationResult> ExportBudgetToExcel(DateTime begin, DateTime end, string filename);
    Task<byte[]> ExportBudgetToExcelBytes(DateTime begin, DateTime end);
}