using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Export.Transaction;

public interface ITransactionExporter
{
    Task<Result> ExportToCsvAsync(ExportOptions options, CancellationToken ct = default);
    Task<Result> ExportToPdfAsync(PdfExportOptions options, CancellationToken ct = default);
}