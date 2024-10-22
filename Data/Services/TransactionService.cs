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
                                              .OrderByDescending(x => x.Id)
                                              .ToListAsync() : new List<Transaction>();
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
}
