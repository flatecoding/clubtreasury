using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Transaction;

public interface ITransactionService
{
    Task<TransactionModel?> GetTransactionByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<TransactionModel>> GetTransactionsForExportAsync(DateTime begin, DateTime end, int cashRegisterId, CancellationToken ct = default);
    Task<IEnumerable<TransactionModel>> GetTransactionsForBudgetExportAsync(DateTime begin, DateTime end, int cashRegisterId, CancellationToken ct = default);

    Task<Result> AddTransactionAsync(TransactionModel entry, CancellationToken ct = default);
    Task<Result> UpdateTransactionAsync(TransactionModel entry, CancellationToken ct = default);
    Task<Result> DeleteTransactionAsync(int id, CancellationToken ct = default);
    Task<HashSet<int>> GetAllDocumentNumbersAsync(int registerId, CancellationToken ct = default);
    Task<int> GetLatestDocumentNumberAsync(int registerId, CancellationToken ct = default);

    Task<PagedResult<TransactionModel>> GetTransactionsPagedAsync(
        PagedRequest request,
        CancellationToken cancellationToken);
}