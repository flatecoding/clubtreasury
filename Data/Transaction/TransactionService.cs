using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Transaction;

public class TransactionService(
    CashDataContext context,
    IAllocationService allocationService,
    ILogger<TransactionService> logger,
    IStringLocalizer<Translation> localizer,
    IResultFactory operationResultFactory) : ITransactionService
{
    private const string ExceptionKey = "Exception";
    private string EntityName => localizer["Transaction"];
    public async Task<TransactionModel?> GetTransactionByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Transactions
            .WithAllocationDetails()
            .WithTransactionDetailsAndPersons()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<HashSet<int>> GetAllDocumentNumbersAsync(int registerId, CancellationToken ct = default)
    {
        return
        [
            ..await context.Transactions.Where(t => t.CashRegisterId == registerId)
                .Select(t => t.Documentnumber)
                .ToListAsync(ct)
        ];
    }

    public async Task<int> GetLatestDocumentNumberAsync(int registerId, CancellationToken ct = default)
    {
        return await context.Transactions
            .Where(t => t.CashRegisterId == registerId && t.Date.HasValue)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Documentnumber)
            .Select(t => t.Documentnumber)
            .FirstOrDefaultAsync(ct);
    }



    public async Task<Result> AddTransactionAsync(TransactionModel entry, CancellationToken ct = default)
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
            logger.LogError(dbEx, "An exception occurred while adding transaction: B{@DocumentNumber} " +
                                     "{@Description}", entry.Documentnumber, entry.Description);
            return operationResultFactory.FailedToAdd(EntityName, localizer[ExceptionKey]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred while adding transaction");
            return operationResultFactory.FailedToAdd(EntityName, localizer[ExceptionKey]);
        }
    }

    public async Task<Result> UpdateTransactionAsync(TransactionModel entry, CancellationToken ct = default)
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
            logger.LogError(dbEx, "An exception occurred while updating transaction: {@DocumentNumber} " +
                                     "{@Description}", entry.Documentnumber, entry.Description);
            return operationResultFactory.FailedToUpdate(EntityName, localizer[ExceptionKey]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred while updating transaction");
            return operationResultFactory.FailedToUpdate(EntityName, localizer[ExceptionKey]);
        }
    }

    public async Task<Result> DeleteTransactionAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var transaction = await context.Transactions.FindAsync([id], ct);
            if (transaction is null)
            {
                logger.LogWarning("Transaction with id '{Id}' could not be found", id);
                return operationResultFactory.NotFound(
                    $"{EntityName} not found",
                    id);
            }

            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync(ct);
            logger.LogInformation("Transaction deleted: B{@DocumentNumber}; Desc.:{@Description}; Sum:{@Sum} " +
                                  "AccMov.: {@AccMov}", transaction.Documentnumber, transaction.Description,
                transaction.Sum, transaction.AccountMovement);
            return operationResultFactory.SuccessDeleted(
                $"{EntityName}: '{transaction.Documentnumber}'",
                id);
        }
        catch (DbUpdateException dbEx)
        {
            logger.LogError(dbEx, "An exception occurred while deleting transaction with id: {Id}", id);
            return operationResultFactory.FailedToDelete(EntityName, localizer[ExceptionKey]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred while deleting transaction with id: {Id}", id);
            return operationResultFactory.FailedToDelete(EntityName, localizer[ExceptionKey]);
        }
    }

    public async Task<IEnumerable<TransactionModel>> GetTransactionsForExportAsync(DateTime begin, DateTime end, int cashRegisterId, CancellationToken ct = default)
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
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<TransactionModel>> GetTransactionsForBudgetExportAsync(
        DateTime begin,
        DateTime end,
        int cashRegisterId,
        CancellationToken ct = default)
    {
        var beginDateOnly = DateOnly.FromDateTime(begin);
        var endDateOnly = DateOnly.FromDateTime(end);

        return await context.Transactions
            .AsSplitQuery()
            .WithAllocationDetails()
            .WithTransactionDetailsAndPersons()
            .Where(t => t.CashRegisterId == cashRegisterId &&
                        t.Date.HasValue &&
                        t.Date.Value >= beginDateOnly &&
                        t.Date.Value <= endDateOnly)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<PagedResult<TransactionModel>> GetTransactionsPagedAsync(
        PagedRequestOptions requestOptions,
        CancellationToken cancellationToken)
    {
        var baseQuery = context.Transactions.AsNoTracking();

        baseQuery = ApplyFilters(baseQuery, requestOptions);

        var totalItems = await baseQuery.CountAsync(cancellationToken);

        var dataQuery = baseQuery.AsSplitQuery()
            .WithAllocationDetails()
            .WithTransactionDetailsAndPersons();

        dataQuery = ApplySorting(dataQuery, requestOptions.SortLabel, requestOptions.SortDirection);

        var items = await dataQuery
            .Skip(requestOptions.Page * requestOptions.PageSize)
            .Take(requestOptions.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TransactionModel>
        {
            TotalItems = totalItems,
            Items = items
        };
    }

    private static IQueryable<TransactionModel> ApplyFilters(IQueryable<TransactionModel> query, PagedRequestOptions requestOptions)
    {
        if (requestOptions.DateStart is not null && requestOptions.DateEnd is not null)
        {
            var start = DateOnly.FromDateTime(requestOptions.DateStart.Value);
            var end = DateOnly.FromDateTime(requestOptions.DateEnd.Value);

            query = query.Where(t => t.Date.HasValue &&
                                     t.Date.Value >= start &&
                                     t.Date.Value <= end);
        }

        if (!string.IsNullOrEmpty(requestOptions.SearchText))
        {
            var term = requestOptions.SearchText.ToLower();
            query = query.Where(t =>
                (t.Description != null && t.Description.ToLower().Contains(term)) ||
                t.Documentnumber.ToString().Contains(term) ||
                t.Allocation.CostCenter.CostUnitName.ToLower().Contains(term) ||
                t.Allocation.Category.Name.ToLower().Contains(term) ||
                (t.Allocation.ItemDetail != null &&
                 t.Allocation.ItemDetail.CostDetails.ToLower().Contains(term)) ||
                t.TransactionDetails.Any(td =>
                    td.Person != null &&
                    td.Person.Name.ToLower().Contains(term)));
        }

        if (requestOptions.PersonId is not null)
        {
            query = query.Where(t =>
                t.TransactionDetails.Any(td => td.PersonId == requestOptions.PersonId));
        }

        return query;
    }

    private static IQueryable<TransactionModel> ApplySorting(
        IQueryable<TransactionModel> query, string? sortLabel, SortDirection direction)
    {
        if (direction == SortDirection.None || string.IsNullOrEmpty(sortLabel))
            return query.OrderByDescending(x => x.Id);

        var descending = direction == SortDirection.Descending;

        return sortLabel switch
        {
            "Date" => Order(query, x => x.Date, descending),
            "DocumentNumber" => Order(query, x => x.Documentnumber, descending),
            "Sum" => Order(query, x => x.Sum, descending),
            "CostCenter" => Order(query, x => x.Allocation.CostCenter.CostUnitName, descending),
            "Category" => Order(query, x => x.Allocation.Category.Name, descending),
            "ItemDetail" => Order(query, x => x.Allocation.ItemDetail != null ? x.Allocation.ItemDetail.CostDetails : null, descending),
            _ => query.OrderByDescending(x => x.Id)
        };
    }

    private static IOrderedQueryable<TransactionModel> Order<TKey>(
        IQueryable<TransactionModel> query,
        Expression<Func<TransactionModel, TKey>> keySelector,
        bool descending)
    {
        return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }

}