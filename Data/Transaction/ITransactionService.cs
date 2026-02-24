using MudBlazor;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Transaction;

public interface ITransactionService
{
    Task<TransactionModel?> GetTransactionByIdAsync(int id);
    Task<IEnumerable<TransactionModel>> GetTransactionsForExport(DateTime begin, DateTime end, int cashRegisterId);
    Task<IEnumerable<TransactionModel>> GetTransactionsForBudgetExport(DateTime begin, DateTime end, int cashRegisterId);

    Task<IOperationResult> AddTransactionAsync(TransactionModel entry, CancellationToken ct = default);
    Task<IOperationResult> UpdateTransactionAsync(TransactionModel entry, CancellationToken ct = default);
    Task<IOperationResult> DeleteTransactionAsync(int id);
    Task<HashSet<int>> GetAllDocumentNumbersAsync(int registerId);
    Task<int> GetLatestDocumentNumberAsync(int registerId);

    // MudBlazor Pagination
    Task<TableData<TransactionModel>> GetTransactionsPaged(
        TableState state,
        CancellationToken cancellationToken,
        DateRange? dateRange,
        string? searchText,
        int? personId);
}