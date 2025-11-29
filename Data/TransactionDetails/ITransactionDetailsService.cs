namespace TTCCashRegister.Data.TransactionDetails;

public interface ITransactionDetailsService
{
    Task<List<TransactionDetailsModel>> GetAllTransactionDetailsAsync();
    Task<TransactionDetailsModel?> GetTransactionDetailsByIdAsync(int id);
    Task<List<TransactionDetailsModel>> GetTransactionDetailsByTransactionIdAsync(int transactionId);
    Task AddTransactionDetailsAsync(TransactionDetailsModel detailsModel);
    Task UpdateTransactionDetailsAsync(TransactionDetailsModel detailsModel);
    Task DeleteAsync(int id);
}