using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TTCCashRegister.Data;
using TTCCashRegister.Data.Models;
using TTCCashRegister.Data.Services;

public class TransactionService
{
    private readonly CashDataContext _context;
    private readonly CashRegisterService _cashRegisterService;
    private readonly SpecialItemService _specialItemService;
    private readonly ExportService _exportService;

    public TransactionService(CashDataContext context, CashRegisterService cashRegisterService, SpecialItemService specialItemService, ExportService exportService)
    {
        _context = context;
        _cashRegisterService = cashRegisterService;
        _specialItemService = specialItemService;
        _exportService = exportService;
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
        return await _context.Transactions.FirstAsync(x => x.Id == id);
    }

    public async Task<bool> AddTransaction(Transaction entry)
    {
        try
        {
            var cashRegister = await _cashRegisterService.GetCashRegisterById(entry.CashRegisterID);
            if (cashRegister == null)
            {
                throw new Exception("Cash Register not found.");
            }

            // Kontobewegung einbuchen
            cashRegister.CurrentBalance += entry.AccountMovement;
            await _cashRegisterService.UpdateCashRegister(cashRegister);

            // Sonderposten verwalten
            if (entry.SpecialItemID.HasValue)
            {
                var sonderposten = await _specialItemService.GetSonderpostenById(entry.SpecialItemID.Value);
                if (sonderposten == null)
                {
                    throw new Exception("Special position not found.");
                }
                sonderposten.Betrag += entry.AccountMovement;
                await _specialItemService.UpdateSonderposten(sonderposten);
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

            var cashRegister = await _cashRegisterService.GetCashRegisterById(entry.CashRegisterID);
            if (cashRegister == null)
            {
                throw new Exception("Cash Register not found.");
            }

            // Adjust the balance for the existing transaction
            cashRegister.CurrentBalance -= existingTransaction.AccountMovement;
            cashRegister.CurrentBalance += entry.AccountMovement;
            await _cashRegisterService.UpdateCashRegister(cashRegister);

            // Sonderposten verwalten
            if (entry.SpecialItemID.HasValue)
            {
                var sonderposten = await _specialItemService.GetSonderpostenById(entry.SpecialItemID.Value);
                if (sonderposten == null)
                {
                    throw new Exception("Sonderposten not found.");
                }
                sonderposten.Betrag -= existingTransaction.AccountMovement;
                sonderposten.Betrag += entry.AccountMovement;
                await _specialItemService.UpdateSonderposten(sonderposten);
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

            var cashRegister = await _cashRegisterService.GetCashRegisterById(transaction.CashRegisterID);
            if (cashRegister == null)
            {
                throw new Exception("Cash Register not found.");
            }

            // Adjust the balance for the deleted transaction
            cashRegister.CurrentBalance -= transaction.AccountMovement;
            await _cashRegisterService.UpdateCashRegister(cashRegister);

            if (transaction.SpecialItemID.HasValue)
            {
                var sonderposten = await _specialItemService.GetSonderpostenById(transaction.SpecialItemID.Value);
                if (sonderposten == null)
                {
                    throw new Exception("Sonderposten not found.");
                }
                sonderposten.Betrag -= transaction.AccountMovement;
                await _specialItemService.UpdateSonderposten(sonderposten);
            }

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

    public async Task<bool> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename)
    {
        return await _exportService.ExportTransactionsToCsv(begin, end, filename);
    }

    public async Task<bool> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename)
    {
        return await _exportService.ExportTransactionsToPdf(begin, end, filename);
    }
}
