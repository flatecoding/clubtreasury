using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Import;

public interface IImportBookingJournalService
{
    Task<IOperationResult> ImportTransactions(Stream? fileStream, string fileName);
}