using MudBlazor;

namespace TTCCashRegister.Data.Transaction;

public interface ITransactionService
{
    Task<List<TransactionModel>?> GetAllTransactions();
    Task<IEnumerable<TransactionModel>> GetTransactionsByDateRange(DateTime start, DateTime end);
    Task<TransactionModel?> GetTransactionByIdAsync(int id);

    Task<bool> AddTransactionAsync(TransactionModel entry, CancellationToken ct = default);
    Task<bool> UpdateTransactionAsync(TransactionModel entry, CancellationToken ct = default);
    Task<bool> DeleteTransactionAsync(int id);

    // Export-Weiterleitungen
    Task<bool> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename);
    Task<bool> ExportBudgetToCsv(DateTime begin, DateTime end, string filename);
    Task<bool> ExportBudgetToExcel(DateTime begin, DateTime end, string filename);
    Task<bool> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename);

    // MudBlazor Pagination
    Task<TableData<TransactionModel>> GetTransactionsPaged(
        TableState state,
        CancellationToken cancellationToken,
        DateRange? dateRange,
        string? searchText,
        int? personId);
    
    Task<TableData<TransactionModel>> GetTransactionsPagedOptimized(
        TableState state,
        CancellationToken cancellationToken,
        DateRange? dateRange,
        string? searchText,
        int? personId);
}