using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Data.Export.Transaction;

public interface IPdfTransactionRenderer
{
    Task RenderTransactionPdfExportAsync(IEnumerable<TransactionModel> transactions, PdfRenderOptions options, CancellationToken ct = default);
}