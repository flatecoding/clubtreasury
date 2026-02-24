using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Data.Export.Transaction;

public interface IPdfTransactionRenderer
{
    Task RenderTransactionPdfExportAsync(IEnumerable<TransactionModel> transactions, DateTime begin, DateTime end,
        string filePath, string cashRegisterName, byte[]? logoData, string? logoContentType, CancellationToken cancellationToken);
}