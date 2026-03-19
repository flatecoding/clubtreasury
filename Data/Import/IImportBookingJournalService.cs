using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Import;

public interface IImportBookingJournalService
{
    Task<Result> ImportTransactionsAsync(Stream? fileStream, string fileName, int cashRegisterId, CancellationToken ct = default);
}