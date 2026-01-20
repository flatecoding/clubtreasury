using MudBlazor;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Transaction;

public interface ITransactionService
{
    Task<List<TransactionModel>?> GetAllTransactions();
    Task<IEnumerable<TransactionModel>> GetTransactionsByDateRange(DateTime start, DateTime end);
    Task<TransactionModel?> GetTransactionByIdAsync(int id);

    Task<IOperationResult> AddTransactionAsync(TransactionModel entry, CancellationToken ct = default);
    Task<IOperationResult> UpdateTransactionAsync(TransactionModel entry, CancellationToken ct = default);
    Task<IOperationResult> DeleteTransactionAsync(int id);

    // Export-Weiterleitungen
    Task<IOperationResult> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename);
    Task<IOperationResult> ExportBudgetToCsv(DateTime begin, DateTime end, string filename);
    Task<IOperationResult> ExportBudgetToExcel(DateTime begin, DateTime end, string filename);
    Task<IOperationResult> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename, CancellationToken ct);

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