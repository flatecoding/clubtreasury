using TTCCashRegister.Data.Transaction;

namespace TTCCashRegister.Data.Export.Transaction;

public interface IPdfTransactionRenderer
{
    Task RenderTransactionPdfExportAsync(IEnumerable<TransactionModel> transactions, DateTime begin, DateTime end,
        string filePath, string cashRegisterName, byte[]? logoData, string? logoContentType, CancellationToken cancellationToken);
}