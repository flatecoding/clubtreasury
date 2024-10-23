using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TTCCashRegister.Data;
using TTCCashRegister.Data.Models;

public class TransactionService
{
    private readonly CashDataContext _context;

    public TransactionService(CashDataContext context)
    {
        _context = context;
    }

    public async Task<List<Transaction>?> GetAllTransactions()
    {
        return _context.Transactions is not null ? await _context.Transactions
                                              .Include(c => c.BasicUnit)
                                              .Include(d => d.CostUnit)
                                              .Include(u => u.UnitDetails)
                                              .OrderByDescending(x => x.Id)
                                              .ToListAsync() : new List<Transaction>();
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int id)
    {
        return await _context.Transactions.FindAsync(id);
    }

    public async Task<bool> AddTransaction(Transaction entry)
    {
        try
        {
            var cashRegister = await _context.CashRegisters.FindAsync(entry.CashRegisterID);
            if (cashRegister == null)
            {
                throw new Exception("Cash Register not found.");
            }

            // Kontobewegung einbuchen
            cashRegister.CurrentBalance += entry.AccountMovement;

            // Sonderposten verwalten
            if (entry.SpecialItemID.HasValue)
            {
                var sonderposten = await _context.SpecialItems.FindAsync(entry.SpecialItemID.Value);
                if (sonderposten == null)
                {
                    throw new Exception("Sonderposten not found.");
                }
                sonderposten.Betrag += entry.Sum;
            }

            await _context.Transactions.AddAsync(entry);
            await _context.SaveChangesAsync();
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

    public async Task<bool> UpdateTransactionAsync(Transaction entry)
    {
        try
        {
            var existingTransaction = await _context.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == entry.Id);
            if (existingTransaction == null)
            {
                throw new Exception("Transaction not found.");
            }

            var cashRegister = await _context.CashRegisters.FindAsync(entry.CashRegisterID);
            if (cashRegister == null)
            {
                throw new Exception("Cash Register not found.");
            }

            // Adjust the balance for the existing transaction
            cashRegister.CurrentBalance -= existingTransaction.AccountMovement;
            cashRegister.CurrentBalance += entry.AccountMovement;

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

            await _context.SaveChangesAsync();
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

    public async Task<bool> DeleteTransactionAsync(int id)
    {
        try
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                throw new Exception("Transaction not found.");
            }

            var cashRegister = await _context.CashRegisters.FindAsync(transaction.CashRegisterID);
            if (cashRegister == null)
            {
                throw new Exception("Cash Register not found.");
            }

            // Adjust the balance for the deleted transaction
            cashRegister.CurrentBalance -= transaction.AccountMovement;

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
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

    public async Task<CashRegister?> GetCashRegisterByIdAsync(int id)
    {
        return await _context.CashRegisters.FindAsync(id);
    }
}
