namespace TTCCashRegister.Data.Import;

public interface IImportBookingJournalService
{
    Task<bool> ImportTransactions(Stream? fileStream);
}