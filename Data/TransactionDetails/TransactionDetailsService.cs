using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.TransactionDetails;

public class TransactionDetailsService(CashDataContext context, ILogger<TransactionDetailsService> logger)
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
        logger.LogInformation("Transaction details added: {@DetailsModel}", detailsModel);
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
        logger.LogInformation("Transaction details updated: {@DetailsModel}", detailsModel);
    }
    
    public async Task DeleteAsync(int id)
    {
        try
        {
            var existing = await context.TransactionDetails.FindAsync(id);
            if (existing == null)
            {
                logger.LogError("Transaction details not found with id: {Id}", id);
                return;
            }

            context.TransactionDetails.Remove(existing);
            await context.SaveChangesAsync();
            logger.LogInformation("Transaction details deleted: {@Existing}", existing);

        }
        catch (DbUpdateException dbUpdateException)
        {
            logger.LogCritical(dbUpdateException, "An exception occurred while deleting transaction with id: {Id}", id);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An exception occurred while deleting transaction with id: {Id}", id);
        }
    }
}