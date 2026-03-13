using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.TransactionDetails;

public interface ITransactionDetailsService
{
    Task<List<TransactionDetailsModel>> GetAllTransactionDetailsAsync(CancellationToken ct = default);
    Task<TransactionDetailsModel?> GetTransactionDetailsByIdAsync(int id, CancellationToken ct = default);
    Task<List<TransactionDetailsModel>> GetTransactionDetailsByTransactionIdAsync(int transactionId, CancellationToken ct = default);
    Task<IOperationResult> AddTransactionDetailsAsync(TransactionDetailsModel detailsModel, CancellationToken ct = default);
    Task<IOperationResult> UpdateTransactionDetailsAsync(TransactionDetailsModel detailsModel, CancellationToken ct = default);
    Task<IOperationResult> DeleteTransactionDetailsAsync(int id, CancellationToken ct = default);
}