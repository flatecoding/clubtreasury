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

    Task<PagedResult<TransactionModel>> GetTransactionsPaged(
        PagedRequest request,
        CancellationToken cancellationToken);
}