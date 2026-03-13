using MudBlazor;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Transaction;

public interface ITransactionService
{
    Task<TransactionModel?> GetTransactionByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<TransactionModel>> GetTransactionsForExport(DateTime begin, DateTime end, int cashRegisterId, CancellationToken ct = default);
    Task<IEnumerable<TransactionModel>> GetTransactionsForBudgetExport(DateTime begin, DateTime end, int cashRegisterId, CancellationToken ct = default);

    Task<IOperationResult> AddTransactionAsync(TransactionModel entry, CancellationToken ct = default);
    Task<IOperationResult> UpdateTransactionAsync(TransactionModel entry, CancellationToken ct = default);
    Task<IOperationResult> DeleteTransactionAsync(int id, CancellationToken ct = default);
    Task<HashSet<int>> GetAllDocumentNumbersAsync(int registerId, CancellationToken ct = default);
    Task<int> GetLatestDocumentNumberAsync(int registerId, CancellationToken ct = default);

    // MudBlazor Pagination
    Task<TableData<TransactionModel>> GetTransactionsPaged(
        TableState state,
        CancellationToken cancellationToken,
        DateRange? dateRange,
        string? searchText,
        int? personId);
}