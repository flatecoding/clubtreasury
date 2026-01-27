using MudBlazor;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Transaction;

public interface ITransactionService
{
    Task<List<TransactionModel>?> GetAllTransactions();
    Task<TransactionModel?> GetTransactionByIdAsync(int id);
    Task<IEnumerable<TransactionModel>> GetTransactionsForExport(DateTime begin, DateTime end);
    Task<IEnumerable<TransactionModel>> GetTransactionsForBudgetExport(DateTime begin, DateTime end);

    Task<IOperationResult> AddTransactionAsync(TransactionModel entry, CancellationToken ct = default);
    Task<IOperationResult> UpdateTransactionAsync(TransactionModel entry, CancellationToken ct = default);
    Task<IOperationResult> DeleteTransactionAsync(int id);
    Task<HashSet<int>> GetAllDocumentNumbersAsync();

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