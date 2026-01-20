using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.TransactionDetails;

public interface ITransactionDetailsService
{
    Task<List<TransactionDetailsModel>> GetAllTransactionDetailsAsync();
    Task<TransactionDetailsModel?> GetTransactionDetailsByIdAsync(int id);
    Task<List<TransactionDetailsModel>> GetTransactionDetailsByTransactionIdAsync(int transactionId);
    Task<IOperationResult> AddTransactionDetailsAsync(TransactionDetailsModel detailsModel);
    Task<IOperationResult> UpdateTransactionDetailsAsync(TransactionDetailsModel detailsModel);
    Task<IOperationResult> DeleteTransactionDetailsAsync(int id);
}