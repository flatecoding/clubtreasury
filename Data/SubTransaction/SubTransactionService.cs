using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.SubTransaction;

public class SubTransactionService(CashDataContext context)
{
    public async Task<List<SubTransactionModel>> GetAllSubTransactionsAsync()
    {
        return await context.SubTransactions
            .Include(st => st.Transaction)
            .Include(st => st.Person)
            .ToListAsync();
    }
    
    public async Task<SubTransactionModel?> GetSubTransactionByIdAsync(int id)
    {
        return await context.SubTransactions
            .Include(st => st.Transaction)
            .Include(st => st.Person)
            .FirstOrDefaultAsync(st => st.Id == id);
    }
    
    public async Task<List<SubTransactionModel>> GetByTransactionIdAsync(int transactionId)
    {
        return await context.SubTransactions
            .Include(st => st.Transaction)
            .Include(st => st.Person)
            .Where(st => st.TransactionId == transactionId)
            .ToListAsync();
    }
    
    public async Task AddSubTransactionAsync(SubTransactionModel model)
    {
        context.SubTransactions.Add(model);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(SubTransactionModel model)
    {
        var existing = await context.SubTransactions.FindAsync(model.Id);
        if (existing == null) return;

        existing.TransactionId = model.TransactionId;
        existing.Description = model.Description;
        existing.Sum = model.Sum;
        existing.PersonId = model.PersonId;

        await context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        var existing = await context.SubTransactions.FindAsync(id);
        if (existing == null) return;

        context.SubTransactions.Remove(existing);
        await context.SaveChangesAsync();
    }
}