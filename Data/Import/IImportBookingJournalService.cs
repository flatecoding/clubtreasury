using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Import;

public interface IImportBookingJournalService
{
    Task<Result> ImportTransactions(Stream? fileStream, string fileName, int cashRegisterId, CancellationToken ct = default);
}