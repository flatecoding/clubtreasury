using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.CashRegister;
using TTCCashRegister.Data.Export;
using TTCCashRegister.Data.SpecialItem;

namespace TTCCashRegister.Data.Transaction;

public class TransactionService(
    CashDataContext context,
    CashRegisterService cashRegisterService,
    SpecialItemService specialItemService,
    ExportService exportService)
{
    public async Task<List<TransactionModel>?> GetAllTransactions()
    {
        return await context.Transactions
            .Include(c => c.BasicUnit)
            .Include(d => d.CostUnit)
            .Include(u => u.UnitDetails)
            .OrderByDescending(x => x.Id)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<IEnumerable<TransactionModel>> GetTransactionsByDateRange(DateTime start, DateTime end)
    {
        return await context.Transactions
            .Include(t => t.CostUnit)
            .Include(t => t.BasicUnit)
            .Include(t => t.UnitDetails)
            .Where(t => t.Date.HasValue &&
                        t.Date.Value >= DateOnly.FromDateTime(start) &&
                        t.Date.Value <= DateOnly.FromDateTime(end))
            .ToListAsync();
    }
    
    public async Task<TransactionModel?> GetTransactionByIdAsync(int id)
    {
        return await context.Transactions.FirstAsync(x => x.Id == id);
    }

    public async Task<bool> AddTransaction(TransactionModel entry)
    {
        try
        {
            var cashRegister = await cashRegisterService.GetCashRegisterById(entry.CashRegisterId);
            if (cashRegister == null)
            {
                throw new Exception("Cash Register not found.");
            }

            // Sonderposten verwalten
            if (entry.SpecialItemId.HasValue)
            {
                var sonderposten = await specialItemService.GetSonderpostenById(entry.SpecialItemId.Value);
                if (sonderposten == null)
                {
                    throw new Exception("Special position not found.");
                }
                sonderposten.Betrag += entry.AccountMovement;
                await specialItemService.UpdateSonderposten(sonderposten);
            }

            await context.Transactions.AddAsync(entry);
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            Debug.WriteLine($"DBUpdateException: {dbEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex}");
            return false;
        }
    }

    public async Task<bool> UpdateTransactionAsync(TransactionModel entry)
    {
        try
        {
            var existingTransaction = await context.Transactions.FirstOrDefaultAsync(t => t.Id == entry.Id);
            if (existingTransaction == null)
            {
                throw new Exception("Transaction not found.");
            }
            
            // Sonderposten verwalten
            if (entry.SpecialItemId.HasValue)
            {
                var sonderposten = await specialItemService.GetSonderpostenById(entry.SpecialItemId.Value);
                if (sonderposten == null)
                {
                    throw new Exception("Sonderposten not found.");
                }
                if(existingTransaction.SpecialItemId.HasValue && 
                   existingTransaction.SpecialItemId.Value != entry.SpecialItemId.Value)
                {
                    sonderposten.Betrag -= existingTransaction.AccountMovement;
                }
                sonderposten.Betrag += entry.AccountMovement;
                await specialItemService.UpdateSonderposten(sonderposten);
            }

            // Update the transaction details
            existingTransaction.Description = entry.Description;
            existingTransaction.AccountMovement = entry.AccountMovement;
            existingTransaction.Date = entry.Date;
            existingTransaction.Documentnumber = entry.Documentnumber;
            existingTransaction.Sum = entry.Sum;
            existingTransaction.CostUnit = entry.CostUnit;
            existingTransaction.BasicUnit = entry.BasicUnit;
            existingTransaction.UnitDetails = entry.UnitDetails;
            existingTransaction.UnitDetailsId = entry.UnitDetailsId;
            existingTransaction.SpecialItem = entry.SpecialItem;
            existingTransaction.SpecialItemId = entry.SpecialItemId;
            
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            Debug.WriteLine($"DBUpdateException: {dbEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteTransactionAsync(int id)
    {
        try
        {
            var transaction = await context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                throw new Exception("Transaction not found.");
            }

            var cashRegister = await cashRegisterService.GetCashRegisterById(transaction.CashRegisterId);
            if (cashRegister == null)
            {
                throw new Exception("Cash Register not found.");
            }

            if (transaction.SpecialItemId.HasValue)
            {
                var sonderposten = await specialItemService
                                                        .GetSonderpostenById(transaction.SpecialItemId.Value);
                if (sonderposten == null) 
                    throw new Exception("Sonderposten not found.");
                
                sonderposten.Betrag -= transaction.AccountMovement;
                await specialItemService.UpdateSonderposten(sonderposten);
            }

            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();
            
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            Debug.WriteLine($"DBUpdateException: {dbEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex}");
            return false;
        }
    }

    public async Task<bool> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename)
    {
        return await exportService.ExportTransactionsToCsv(begin, end, filename);
    }
    public async Task<bool> ExportBudgetToCsv(DateTime begin, DateTime end, string filename)
    {
        return await exportService.ExportBudgetToCsv(begin, end, filename);
    }

    public async Task<bool> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename)
    {
        return await exportService.ExportTransactionsToPdf(begin, end, filename);
    }
}