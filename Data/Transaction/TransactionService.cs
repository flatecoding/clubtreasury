using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using TTCCashRegister.Data.Accounts;
using TTCCashRegister.Data.CashRegister;
using TTCCashRegister.Data.Export;

namespace TTCCashRegister.Data.Transaction;

public class TransactionService(
    CashDataContext context,
    CashRegisterService cashRegisterService,
    ExportService exportService,
    AccountsService accountsService)
{
    public async Task<List<TransactionModel>?> GetAllTransactions()
    {
        return await context.Transactions
            .Include(t => t.Accounts)
                .ThenInclude(a => a.CostCenter)
            .Include(t => t.Accounts)
                .ThenInclude(a => a.Category)
            .Include(t => t.Accounts)
                .ThenInclude(a => a.UnitDetails)
            .Include(t => t.SubTransactions)
                .ThenInclude(st => st.Person)
            .OrderByDescending(x => x.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<TransactionModel>> GetTransactionsByDateRange(DateTime start, DateTime end)
    {
        return await context.Transactions
            .Include(t => t.Accounts)
                .ThenInclude(a => a.CostCenter)
            .Include(t => t.Accounts)
                .ThenInclude(a => a.Category)
            .Include(t => t.Accounts)
                .ThenInclude(a => a.UnitDetails)
            .Where(t => t.Date.HasValue &&
                        t.Date.Value >= DateOnly.FromDateTime(start) &&
                        t.Date.Value <= DateOnly.FromDateTime(end))
            .ToListAsync();
    }

    public async Task<TransactionModel?> GetTransactionByIdAsync(int id)
    {
        return await context.Transactions
            .Include(t => t.Accounts)
            .ThenInclude(a => a.CostCenter)
            .Include(t => t.Accounts)
            .ThenInclude(a => a.Category)
            .Include(t => t.Accounts)
            .ThenInclude(a => a.UnitDetails)
            .Include(t => t.SubTransactions)
            .ThenInclude(st => st.Person)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    


    public async Task<bool> AddTransaction(TransactionModel entry)
    {
        try
        {
            var cashRegister = await cashRegisterService.GetCashRegisterById(entry.CashRegisterId);
            if (cashRegister is null)
                throw new Exception($"No cash register with '{entry.CashRegisterId}' found.");

            // ✅ Account prüfen oder neu anlegen
            var account = await accountsService.EnsureAccountExistsAsync(entry.Accounts);
            entry.AccountsId = account.Id;
            entry.Accounts = null;

            await context.Transactions.AddAsync(entry);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in AddTransaction: {ex}");
            return false;
        }
    }


    public async Task<bool> UpdateTransactionAsync(TransactionModel entry)
    {
        try
        {
            var existingTransaction = await context.Transactions
                .FirstOrDefaultAsync(t => t.Id == entry.Id);

            if (existingTransaction == null)
                throw new Exception("Transaction not found.");

            // Allocation prüfen oder anlegen
            var account = await context.Accounts.FirstOrDefaultAsync(a =>
                a.CostCenterId == entry.Accounts.CostCenterId &&
                a.CategoryId == entry.Accounts.CategoryId &&
                a.UnitDetailsId == entry.Accounts.UnitDetailsId);

            if (account == null)
            {
                account = entry.Accounts;
                context.Accounts.Add(account);
                await context.SaveChangesAsync();
            }

            // Transaction aktualisieren
            existingTransaction.Description = entry.Description;
            existingTransaction.AccountMovement = entry.AccountMovement;
            existingTransaction.Date = entry.Date;
            existingTransaction.Documentnumber = entry.Documentnumber;
            existingTransaction.Sum = entry.Sum;
            existingTransaction.AccountsId = account.Id;
            existingTransaction.Accounts = null; // nur Id speichern
            existingTransaction.SpecialItemId = entry.SpecialItemId;
            existingTransaction.CashRegisterId = entry.CashRegisterId;

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
            if (transaction is null)
                throw new Exception("Transaction not found.");

            var cashRegister = await cashRegisterService.GetCashRegisterById(transaction.CashRegisterId);
            if (cashRegister is null)
                throw new Exception($"Cash Register with Id: '{id}' not found.");

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
        => await exportService.ExportTransactionsToCsv(begin, end, filename);

    public async Task<bool> ExportBudgetToCsv(DateTime begin, DateTime end, string filename)
        => await exportService.ExportBudgetToCsv(begin, end, filename);

    public async Task<bool> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename)
        => await exportService.ExportTransactionsToPdf(begin, end, filename);

    public async Task<TableData<TransactionModel>> GetTransactionsPaged(
        TableState state,
        CancellationToken cancellationToken,
        DateRange? dateRange,
        string? searchText,
        int? personId)
    {
        var query = context.Transactions
            .Include(t => t.Accounts)
                .ThenInclude(a => a.CostCenter)
            .Include(t => t.Accounts)
                .ThenInclude(a => a.Category)
            .Include(t => t.Accounts)
                .ThenInclude(a => a.UnitDetails)
            .Include(t => t.SubTransactions)
                .ThenInclude(st => st.Person)
            .AsNoTracking();

        // 🔍 Datum
        if (dateRange?.Start is not null && dateRange?.End is not null)
        {
            var start = dateRange.Start.Value.Date;
            var end = dateRange.End.Value.Date;
            query = query.Where(t => t.Date.HasValue &&
                                     t.Date.Value >= DateOnly.FromDateTime(start) &&
                                     t.Date.Value <= DateOnly.FromDateTime(end));
        }

        // 🔎 Suchtext
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.ToLower();
            query = query.Where(x =>
                (x.Description != null && x.Description.ToLower().Contains(term)) ||
                x.Documentnumber.ToString().Contains(term) ||
                (x.Accounts.CostCenter.CostUnitName.ToLower().Contains(term)) ||
                (x.Accounts.Category.Name.ToLower().Contains(term)) ||
                (x.Accounts.UnitDetails != null && x.Accounts.UnitDetails.CostDetails.ToLower().Contains(term)) ||
                x.SubTransactions.Any(st => st.Person != null && st.Person.Name.ToLower().Contains(term))
            );
        }

        // 👤 Person-Filter
        if (personId is not null)
        {
            query = query.Where(t =>
                t.SubTransactions.Any(st => st.PersonId == personId));
        }

        // 🔽 Sortierung
        query = state.SortLabel switch
        {
            "Date" => state.SortDirection == SortDirection.Descending
                ? query.OrderByDescending(x => x.Date)
                : query.OrderBy(x => x.Date),
            "DocumentNumber" => state.SortDirection == SortDirection.Descending
                ? query.OrderByDescending(x => x.Documentnumber)
                : query.OrderBy(x => x.Documentnumber),
            "Sum" => state.SortDirection == SortDirection.Descending
                ? query.OrderByDescending(x => x.Sum)
                : query.OrderBy(x => x.Sum),
            _ => query.OrderByDescending(x => x.Id)
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(state.Page * state.PageSize)
            .Take(state.PageSize)
            .ToListAsync(cancellationToken);

        return new TableData<TransactionModel>
        {
            TotalItems = totalItems,
            Items = items
        };
    }
}
