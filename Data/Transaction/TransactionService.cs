using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.CashRegister;
using TTCCashRegister.Data.Export;

namespace TTCCashRegister.Data.Transaction;

public class TransactionService(
    CashDataContext context,
    IExportService exportService,
    IAllocationService allocationService,
    ILogger<TransactionService> logger) : ITransactionService
{
    public async Task<List<TransactionModel>?> GetAllTransactions()
    {
        return await context.Transactions
            .Include(t => t.Allocation)
                .ThenInclude(a => a.CostCenter)
            .Include(t => t.Allocation)
                .ThenInclude(a => a.Category)
            .Include(t => t.Allocation)
                .ThenInclude(a => a.ItemDetail)
            .Include(t => t.TransactionDetails)
                .ThenInclude(st => st.Person)
            .OrderByDescending(x => x.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<TransactionModel>> GetTransactionsByDateRange(DateTime start, DateTime end)
    {
        return await context.Transactions
            .Include(t => t.Allocation)
                .ThenInclude(a => a.CostCenter)
            .Include(t => t.Allocation)
                .ThenInclude(a => a.Category)
            .Include(t => t.Allocation)
                .ThenInclude(a => a.ItemDetail)
            .Where(t => t.Date.HasValue &&
                        t.Date.Value >= DateOnly.FromDateTime(start) &&
                        t.Date.Value <= DateOnly.FromDateTime(end))
            .ToListAsync();
    }

    public async Task<TransactionModel?> GetTransactionByIdAsync(int id)
    {
        return await context.Transactions
            .Include(t => t.Allocation)
            .ThenInclude(a => a.CostCenter)
            .Include(t => t.Allocation)
            .ThenInclude(a => a.Category)
            .Include(t => t.Allocation)
            .ThenInclude(a => a.ItemDetail)
            .Include(t => t.TransactionDetails)
            .ThenInclude(st => st.Person)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    


    public async Task<bool> AddTransactionAsync(TransactionModel entry, CancellationToken ct = default)
    {
        try
        {
            // 1) Guard: Kasse muss existieren
            var cashRegisterExists = await context.CashRegisters
                .AnyAsync(cr => cr.Id == entry.CashRegisterId, ct);

            if (!cashRegisterExists)
                throw new InvalidOperationException($"No cash register with '{entry.CashRegisterId}' found.");

            // 2) Allocation sicherstellen (ohne Save)
            var account = await allocationService.EnsureAllocationExistsAsync(entry.Allocation, ct);
            
            var tx = new TransactionModel
            {
                Description     = entry.Description,
                AccountMovement = entry.AccountMovement,
                Date            = entry.Date,
                Documentnumber  = entry.Documentnumber,
                Sum             = entry.Sum,
                SpecialItemId   = entry.SpecialItemId,
                CashRegisterId  = entry.CashRegisterId,
                Allocation      = account // Navigation setzen
            };

            context.Transactions.Add(tx);
            
            await context.SaveChangesAsync(ct);
            logger.LogInformation("Transaction added: B{@DocumentNumber}; Desc.:{@Description}; Sum:{@Sum} " +
                                  "AccMov.: {@AccMov}", tx.Documentnumber, tx.Description, tx.Sum, tx.AccountMovement);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            logger.LogCritical(dbEx, "An exception occurred while adding transaction: B{@DocumentNumber} " +
                                     "{@Description}", entry.Documentnumber, entry.Description);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An exception occurred while adding transaction");
            return false;
        }
    }
    
    public async Task<bool> UpdateTransactionAsync(TransactionModel entry, CancellationToken ct = default)
    {
        try
        {
            var tx = await context.Transactions
                .FirstOrDefaultAsync(t => t.Id == entry.Id, ct);

            if (tx == null)
                throw new KeyNotFoundException("Transaction not found.");
            
            var account = await allocationService.EnsureAllocationExistsAsync(entry.Allocation, ct);
            
            tx.Description     = entry.Description;
            tx.AccountMovement = entry.AccountMovement;
            tx.Date            = entry.Date;
            tx.Documentnumber  = entry.Documentnumber;
            tx.Sum             = entry.Sum;
            tx.SpecialItemId   = entry.SpecialItemId;
            tx.CashRegisterId  = entry.CashRegisterId;
            tx.Allocation   = account;    
            tx.AllocationId = account.Id;
            
            await context.SaveChangesAsync(ct);
            logger.LogInformation("Transaction updated: B{@DocumentNumber}; Desc.:{@Description}; Sum:{@Sum} " +
                                  "AccMov.: {@AccMov}", tx.Documentnumber, tx.Description, tx.Sum, tx.AccountMovement);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            logger.LogCritical(dbEx, "An exception occurred while updating transaction: {@DocumentNumber} " +
                                     "{@Description}", entry.Documentnumber, entry.Description);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An exception occurred while updating transaction");
            return false;
        }
    }
    
    public async Task<bool> DeleteTransactionAsync(int id)
    {
        try
        {
            var transaction = await context.Transactions.FindAsync(id);
            if (transaction is null)
            {
                logger.LogError("Transaction with id '{Id}' could not be found", id);
                return false;
            }
  
            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();
            logger.LogInformation("Transaction deleted: B{@DocumentNumber}; Desc.:{@Description}; Sum:{@Sum} " +
                                  "AccMov.: {@AccMov}", transaction.Documentnumber, transaction.Description, 
                transaction.Sum, transaction.AccountMovement);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            logger.LogCritical(dbEx, "An exception occurred while deleting transaction with id: {Id}", id);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An exception occurred while deleting transaction with id: {Id}", id);
            return false;
        }
    }

    public async Task<bool> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename)
        => await exportService.ExportTransactionsToCsv(begin, end, filename);

    public async Task<bool> ExportBudgetToCsv(DateTime begin, DateTime end, string filename)
        => await exportService.ExportBudgetToCsv(begin, end, filename);
    
    public async Task<bool> ExportBudgetToExcel(DateTime begin, DateTime end, string filename)
    => await exportService.ExportBudgetToExcelWithCharts(begin, end, filename);
    
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
            .Include(t => t.Allocation)
                .ThenInclude(a => a.CostCenter)
            .Include(t => t.Allocation)
                .ThenInclude(a => a.Category)
            .Include(t => t.Allocation)
                .ThenInclude(a => a.ItemDetail)
            .Include(t => t.TransactionDetails)
                .ThenInclude(st => st.Person)
            .AsNoTracking();

        //Datum
        if (dateRange?.Start is not null && dateRange?.End is not null)
        {
            var start = dateRange.Start.Value.Date;
            var end = dateRange.End.Value.Date;
            query = query.Where(t => t.Date.HasValue &&
                                     t.Date.Value >= DateOnly.FromDateTime(start) &&
                                     t.Date.Value <= DateOnly.FromDateTime(end));
        }

        //Suchtext
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.ToLower();
            query = query.Where(x =>
                (x.Description != null && x.Description.ToLower().Contains(term)) ||
                x.Documentnumber.ToString().Contains(term) ||
                (x.Allocation.CostCenter.CostUnitName.ToLower().Contains(term)) ||
                (x.Allocation.Category.Name.ToLower().Contains(term)) ||
                (x.Allocation.ItemDetail != null && x.Allocation.ItemDetail.CostDetails.ToLower().Contains(term)) ||
                x.TransactionDetails.Any(st => st.Person != null && st.Person.Name.ToLower().Contains(term))
            );
        }

        //Person-Filter
        if (personId is not null)
        {
            query = query.Where(t =>
                t.TransactionDetails.Any(st => st.PersonId == personId));
        }

        // Sortierung
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
