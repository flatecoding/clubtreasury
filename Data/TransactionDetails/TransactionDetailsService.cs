using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.TransactionDetails;

public class TransactionDetailsService(CashDataContext context)
{
    public async Task<List<TransactionDetailsModel>> GetAllTransactionDetailsAsync()
    {
        return await context.TransactionDetails
            .Include(st => st.Transaction)
            .Include(st => st.Person)
            .ToListAsync();
    }
    
    public async Task<TransactionDetailsModel?> GetTransactionDetailsByIdAsync(int id)
    {
        return await context.TransactionDetails
            .Include(st => st.Transaction)
            .Include(st => st.Person)
            .FirstOrDefaultAsync(st => st.Id == id);
    }
    
    public async Task<List<TransactionDetailsModel>> GetTransactionDetailsByTransactionIdAsync(int transactionId)
    {
        return await context.TransactionDetails
            .Include(st => st.Transaction)
            .Include(st => st.Person)
            .Where(st => st.TransactionId == transactionId)
            .ToListAsync();
    }
    
    public async Task AddTransactionDetailsAsync(TransactionDetailsModel detailsModel)
    {
        context.TransactionDetails.Add(detailsModel);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateTransactionDetailsAsync(TransactionDetailsModel detailsModel)
    {
        var existing = await context.TransactionDetails.FindAsync(detailsModel.Id);
        if (existing == null) return;

        existing.TransactionId = detailsModel.TransactionId;
        existing.Description = detailsModel.Description;
        existing.Sum = detailsModel.Sum;
        existing.PersonId = detailsModel.PersonId;

        await context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        var existing = await context.TransactionDetails.FindAsync(id);
        if (existing == null) return;

        context.TransactionDetails.Remove(existing);
        await context.SaveChangesAsync();
    }
}