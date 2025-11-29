namespace TTCCashRegister.Data.Export;

public interface IExportService
{
    Task<bool> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename);
    Task<bool> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename);
    Task<bool> ExportBudgetToCsv(DateTime begin, DateTime end, string filename);
    Task<bool> ExportBudgetToExcelWithCharts(DateTime begin, DateTime end, string filename);
    Task<byte[]> ExportBudgetToExcelBytes(DateTime begin, DateTime end);
}