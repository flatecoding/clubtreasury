using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.Services;

namespace TTCCashRegister.Data.Transaction;

public class TransactionService(
    CashDataContext context,
    CashRegisterService cashRegisterService,
    SpecialItemService specialItemService,
    ExportService exportService)
{
    public async Task<List<TransactionModel>?> GetAllTransactions()
    {
        return context.Transactions is not null ? await context.Transactions
            .Include(c => c.BasicUnit)
            .Include(d => d.CostUnit)
            .Include(u => u.UnitDetails)
            .OrderByDescending(x => x.Id)
            .ToListAsync() : new List<TransactionModel>();
    }
    
    

    public async Task<TransactionModel?> GetTransactionByIdAsync(int id)
    {
        return await context.Transactions.FirstAsync(x => x.Id == id);
    }

    public async Task<bool> AddTransaction(Data.Transaction.TransactionModel entry)
    {
        try
        {
            var cashRegister = await cashRegisterService.GetCashRegisterById(entry.CashRegisterID);
            if (cashRegister == null)
            {
                throw new Exception("Cash Register not found.");
            }

            // Kontobewegung einbuchen
            cashRegister.CurrentBalance += entry.AccountMovement;
            await cashRegisterService.UpdateCashRegister(cashRegister);

            // Sonderposten verwalten
            if (entry.SpecialItemID.HasValue)
            {
                var sonderposten = await specialItemService.GetSonderpostenById(entry.SpecialItemID.Value);
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
            var existingTransaction = await context.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == entry.Id);
            if (existingTransaction == null)
            {
                throw new Exception("Transaction not found.");
            }

            var cashRegister = await cashRegisterService.GetCashRegisterById(entry.CashRegisterID);
            if (cashRegister == null)
            {
                throw new Exception("Cash Register not found.");
            }

            // Adjust the balance for the existing transaction
            cashRegister.CurrentBalance -= existingTransaction.AccountMovement;
            cashRegister.CurrentBalance += entry.AccountMovement;
            await cashRegisterService.UpdateCashRegister(cashRegister);

            // Sonderposten verwalten
            if (entry.SpecialItemID.HasValue)
            {
                var sonderposten = await specialItemService.GetSonderpostenById(entry.SpecialItemID.Value);
                if (sonderposten == null)
                {
                    throw new Exception("Sonderposten not found.");
                }
                sonderposten.Betrag -= existingTransaction.AccountMovement;
                sonderposten.Betrag += entry.AccountMovement;
                await specialItemService.UpdateSonderposten(sonderposten);
            }

            // Update the transaction details
            existingTransaction.CashRegister = cashRegister;
            existingTransaction.CashRegisterID = cashRegister.ID;
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
            existingTransaction.SpecialItemID = entry.SpecialItemID;

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

            var cashRegister = await cashRegisterService.GetCashRegisterById(transaction.CashRegisterID);
            if (cashRegister == null)
            {
                throw new Exception("Cash Register not found.");
            }

            // Adjust the balance for the deleted transaction
            cashRegister.CurrentBalance -= transaction.AccountMovement;
            await cashRegisterService.UpdateCashRegister(cashRegister);

            if (transaction.SpecialItemID.HasValue)
            {
                var sonderposten = await specialItemService.GetSonderpostenById(transaction.SpecialItemID.Value);
                if (sonderposten == null)
                {
                    throw new Exception("Sonderposten not found.");
                }
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