using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Import;

public interface IImportBookingJournalService
{
    Task<IOperationResult> ImportTransactions(Stream? fileStream, string fileName, int cashRegisterId);
}