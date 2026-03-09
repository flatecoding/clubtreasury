using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MudBlazor;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Transaction;

public class TransactionService(
    CashDataContext context,
    IAllocationService allocationService,
    ILogger<TransactionService> logger,
    IStringLocalizer<Translation> localizer,
    IOperationResultFactory operationResultFactory) : ITransactionService
{
    private string EntityName => localizer["Transaction"];
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
    
    public async Task<HashSet<int>> GetAllDocumentNumbersAsync(int registerId)
    {
        return
        [
            ..await context.Transactions.Where(t => t.CashRegisterId == registerId)
                .Select(t => t.Documentnumber)
                .ToListAsync()
        ];
    }

    public async Task<int> GetLatestDocumentNumberAsync(int registerId)
    {
        return await context.Transactions
            .Where(t => t.CashRegisterId == registerId && t.Date.HasValue)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Documentnumber)
            .Select(t => t.Documentnumber)
            .FirstOrDefaultAsync();
    }
    


    public async Task<IOperationResult> AddTransactionAsync(TransactionModel entry, CancellationToken ct = default)
    {
        try
        {
            if (!await context.CashRegisters
                        .AnyAsync(cr => cr.Id == entry.CashRegisterId, ct))
                {
                    return operationResultFactory.NotFound(
                        EntityName, entry.CashRegisterId);
                }
            
                if (await context.Transactions
                        .AnyAsync(t => t.Documentnumber == entry.Documentnumber
                                       && t.CashRegisterId == entry.CashRegisterId, ct))
                {
                    logger.LogWarning(
                        "Transaction with DocumentNumber {DocumentNumber} already exists.",
                        entry.Documentnumber);

                    return operationResultFactory.AlreadyExists(
                        EntityName,
                        $"{localizer["DocumentNumber"]} '{entry.Documentnumber}'");
                }
                
                var allocation = await allocationService.GetRequiredAllocationAsync(
                    entry.AllocationId > 0
                        ? entry.AllocationId
                        : throw new InvalidOperationException("AllocationId missing on transaction."),
                    ct);

                var tx = new TransactionModel
                {
                    Description     = entry.Description,
                    AccountMovement = entry.AccountMovement,
                    Date            = entry.Date,
                    Documentnumber  = entry.Documentnumber,
                    Sum             = entry.Sum,
                    SpecialItemId   = entry.SpecialItemId,
                    CashRegisterId  = entry.CashRegisterId,
                    AllocationId      = allocation.Id
                };

                context.Transactions.Add(tx);
                await context.SaveChangesAsync(ct);

                logger.LogInformation(
                    "Transaction added: B{DocumentNumber}; Desc:{Description}; Sum:{Sum}; AccMov:{AccMov}",
                    tx.Documentnumber, tx.Description, tx.Sum, tx.AccountMovement);

                return operationResultFactory.SuccessAdded(
                    $"{EntityName}: '{tx.Documentnumber}",
                    tx.Id);
        }
        catch (DbUpdateException dbEx)
        {
            logger.LogCritical(dbEx, "An exception occurred while adding transaction: B{@DocumentNumber} " +
                                     "{@Description}", entry.Documentnumber, entry.Description);
            return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An exception occurred while adding transaction");
            return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
        }
    }
    
    public async Task<IOperationResult> UpdateTransactionAsync(TransactionModel entry, CancellationToken ct = default)
    {
        try
        {
            var existing = await context.Transactions
                .Include(t => t.Allocation)
                .FirstOrDefaultAsync(t => t.Id == entry.Id, ct);

            if (existing == null)
            {
                return operationResultFactory.NotFound(
                    $"{EntityName} not found",
                    entry.Id);
            }
            if (await context.Transactions.AnyAsync(t =>
                    t.Documentnumber == entry.Documentnumber &&
                    t.CashRegisterId == entry.CashRegisterId &&
                    t.Id != entry.Id, ct))
            {
                return operationResultFactory.AlreadyExists(
                    EntityName,
                    $"{localizer["DocumentNumber"]} '{entry.Documentnumber}'");
            }
            
            var allocation = await allocationService.GetRequiredAllocationAsync(entry.AllocationId, ct);

            existing.Description     = entry.Description;
            existing.AccountMovement = entry.AccountMovement;
            existing.Date            = entry.Date;
            existing.Documentnumber  = entry.Documentnumber;
            existing.Sum             = entry.Sum;
            existing.SpecialItemId   = entry.SpecialItemId;
            existing.CashRegisterId  = entry.CashRegisterId;
            existing.AllocationId    = entry.AllocationId;
            existing.Allocation =  allocation;

            await context.SaveChangesAsync(ct);
            logger.LogInformation("Transaction updated: B{@DocumentNumber}; Desc.:{@Description}; Sum:{@Sum} " +
                                  "AccMov.: {@AccMov}", existing.Documentnumber, existing.Description, existing.Sum, 
                                   existing.AccountMovement);
            return operationResultFactory.SuccessUpdated(
                $"{EntityName}: '{existing.Documentnumber}'", existing.Id);
        }
        catch (DbUpdateException dbEx)
        {
            logger.LogCritical(dbEx, "An exception occurred while updating transaction: {@DocumentNumber} " +
                                     "{@Description}", entry.Documentnumber, entry.Description);
            return operationResultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An exception occurred while updating transaction");
            return operationResultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
        }
    }
    
    public async Task<IOperationResult> DeleteTransactionAsync(int id)
    {
        try
        {
            var transaction = await context.Transactions.FindAsync(id);
            if (transaction is null)
            {
                logger.LogError("Transaction with id '{Id}' could not be found", id);
                return operationResultFactory.NotFound(
                    $"{EntityName} not found",
                    id);
            }
  
            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();
            logger.LogInformation("Transaction deleted: B{@DocumentNumber}; Desc.:{@Description}; Sum:{@Sum} " +
                                  "AccMov.: {@AccMov}", transaction.Documentnumber, transaction.Description, 
                transaction.Sum, transaction.AccountMovement);
            return operationResultFactory.SuccessDeleted(
                $"{EntityName}: '{transaction.Documentnumber}'",
                id);
        }
        catch (DbUpdateException dbEx)
        {
            logger.LogCritical(dbEx, "An exception occurred while deleting transaction with id: {Id}", id);
            return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An exception occurred while deleting transaction with id: {Id}", id);
            return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
        }
    }
    
    public async Task<IEnumerable<TransactionModel>> GetTransactionsForExport(DateTime begin, DateTime end, int cashRegisterId)
    {
        return await context.Transactions
            .AsNoTracking()
            .Where(t => t.CashRegisterId == cashRegisterId &&
                        t.Date.HasValue &&
                        t.Date.Value >= DateOnly.FromDateTime(begin) &&
                        t.Date.Value <= DateOnly.FromDateTime(end))
            .Select(t => new TransactionModel
            {
                Date = t.Date,
                Documentnumber = t.Documentnumber,
                Description = t.Description,
                Sum = t.Sum,
                AccountMovement = t.AccountMovement
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<TransactionModel>> GetTransactionsForBudgetExport(
        DateTime begin,
        DateTime end,
        int cashRegisterId)
    {
        var beginDateOnly = DateOnly.FromDateTime(begin);
        var endDateOnly = DateOnly.FromDateTime(end);

        return await context.Transactions
            .AsSplitQuery()
            .Include(t => t.Allocation).ThenInclude(a => a.CostCenter)
            .Include(t => t.Allocation).ThenInclude(a => a.Category)
            .Include(t => t.Allocation).ThenInclude(a => a.ItemDetail)
            .Include(t => t.TransactionDetails).ThenInclude(st => st.Person)
            .Where(t => t.CashRegisterId == cashRegisterId &&
                        t.Date.HasValue &&
                        t.Date.Value >= beginDateOnly &&
                        t.Date.Value <= endDateOnly)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<TableData<TransactionModel>> GetTransactionsPaged(
        TableState state,
        CancellationToken cancellationToken,
        DateRange? dateRange,
        string? searchText,
        int? personId)
    {
        var baseQuery = context.Transactions.AsNoTracking();
        if (dateRange?.Start is not null && dateRange.End is not null)
        {
            var start = DateOnly.FromDateTime(dateRange.Start.Value);
            var end = DateOnly.FromDateTime(dateRange.End.Value);
            
            baseQuery = baseQuery.Where(t => t.Date.HasValue &&
                                             t.Date.Value >= start &&
                                             t.Date.Value <= end);
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            var term = searchText.ToLower();
            baseQuery = baseQuery.Where(t =>
                (t.Description != null && t.Description.ToLower().Contains(term)) ||
                t.Documentnumber.ToString().Contains(term) ||
                t.Allocation.CostCenter.CostUnitName.ToLower().Contains(term) ||
                t.Allocation.Category.Name.ToLower().Contains(term) ||
                (t.Allocation.ItemDetail != null &&
                 t.Allocation.ItemDetail.CostDetails.ToLower().Contains(term)) ||
                t.TransactionDetails.Any(st =>
                    st.Person != null &&
                    st.Person.Name.ToLower().Contains(term)));
        }

        if (personId is not null)
        {
            baseQuery = baseQuery.Where(t =>
                t.TransactionDetails.Any(st => st.PersonId == personId)
            );
        }
        
        var totalItems = await baseQuery.CountAsync(cancellationToken);

        IQueryable<TransactionModel> dataQuery = baseQuery.AsSplitQuery()
            .Include(t => t.Allocation)
                .ThenInclude(a => a.CostCenter)
            .Include(t => t.Allocation)
                .ThenInclude(a => a.Category)
            .Include(t => t.Allocation)
                .ThenInclude(a => a.ItemDetail)
            .Include(t => t.TransactionDetails)
                .ThenInclude(td => td.Person);

        dataQuery = state.SortLabel switch
        {
            "Date" => dataQuery.OrderByDirection(state.SortDirection, x => x.Date),
            "DocumentNumber" => dataQuery.OrderByDirection(state.SortDirection, x => x.Documentnumber),
            "Sum" => dataQuery.OrderByDirection(state.SortDirection, x => x.Sum),
            "CostCenter" => dataQuery.OrderByDirection(state.SortDirection, x => x.Allocation.CostCenter.CostUnitName),
            "Category" => dataQuery.OrderByDirection(state.SortDirection, x => x.Allocation.Category.Name),
            "ItemDetail" => dataQuery.OrderByDirection(state.SortDirection, x =>
                x.Allocation.ItemDetail != null ? x.Allocation.ItemDetail.CostDetails : null),
            _ => dataQuery.OrderByDescending(x => x.Id)
        };

        var items = await dataQuery
            .Skip(state.Page * state.PageSize)
            .Take(state.PageSize)
            .ToListAsync(cancellationToken);

        var tableData = new TableData<TransactionModel>()
        {
            TotalItems = totalItems,
            Items = items
        };
        
        return tableData;
    }
    
}
