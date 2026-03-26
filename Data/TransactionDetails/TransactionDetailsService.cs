using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.TransactionDetails;

public class TransactionDetailsService(CashDataContext context, ILogger<TransactionDetailsService> logger,
    IStringLocalizer<Translation> localizer, IResultFactory operationResultFactory)
    : ITransactionDetailsService
{
    private string EntityName => localizer["TransactionDetails"];
    public async Task<List<TransactionDetailsModel>> GetAllTransactionDetailsAsync(CancellationToken ct = default)
    {
        return await context.TransactionDetails
            .WithTransactionAndPerson()
            .ToListAsync(ct);
    }

    public async Task<TransactionDetailsModel?> GetTransactionDetailsByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.TransactionDetails
            .WithTransactionAndPerson()
            .FirstOrDefaultAsync(st => st.Id == id, ct);
    }

    public async Task<List<TransactionDetailsModel>> GetTransactionDetailsByTransactionIdAsync(int transactionId, CancellationToken ct = default)
    {
        return await context.TransactionDetails
            .WithTransactionAndPerson()
            .Where(st => st.TransactionId == transactionId)
            .ToListAsync(ct);
    }

    public async Task<Result> AddTransactionDetailsAsync(TransactionDetailsModel detailsModel, CancellationToken ct = default)
    {
        try
        {
            context.TransactionDetails.Add(detailsModel);
            await context.SaveChangesAsync(ct);
            logger.LogInformation("Transaction details added: {@DetailsModelDescription}, Sum: {@Sum}" +
                                  " Name: {@Name}" ,
                detailsModel.Description,  decimal.Round(detailsModel.Sum, 2), detailsModel.Person?.Name ?? "null");
            return operationResultFactory.SuccessAdded($"{EntityName}: '{detailsModel.Description}'", detailsModel.Person?.Name ?? "null");

        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to add transaction details");
            return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
        }

    }

    public async Task<Result> UpdateTransactionDetailsAsync(TransactionDetailsModel detailsModel, CancellationToken ct = default)
    {
        try
        {
            var existing = await context.TransactionDetails.FindAsync([detailsModel.Id], ct);
            if (existing is null)
            {
                logger.LogWarning("Transaction details with Id {Id} not found", detailsModel.Id);
                return operationResultFactory.NotFound(EntityName, detailsModel.Id);
            }

            existing.Description = detailsModel.Description;
            existing.Sum = detailsModel.Sum;
            existing.PersonId = detailsModel.PersonId;
            existing.DocumentNumber = detailsModel.DocumentNumber;

            await context.SaveChangesAsync(ct);
            logger.LogInformation("Transaction details updated: {@DetailsModelDescription}, Sum: {@Sum}" +
                                  " Name: {@Name}" ,
                existing.Description, decimal.Round(existing.Sum, 2), detailsModel.Person?.Name ?? "null");
            return operationResultFactory.SuccessUpdated($"{EntityName}: '{existing.Description}'", existing.Id);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update transaction details with Id {Id}", detailsModel.Id);
            return operationResultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
        }
    }

    public async Task<Result> DeleteTransactionDetailsAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var existing = await GetTransactionDetailsByIdAsync(id, ct);
            if (existing is null)
            {
                logger.LogError("Transaction details not found with id: {Id}", id);
                return operationResultFactory.NotFound(EntityName, id);
            }
            context.TransactionDetails.Remove(existing);
            await context.SaveChangesAsync(ct);
            logger.LogInformation("Transaction details deleted: {@DetailsModelDescription}, Sum: {@Sum} Name: {@Name}",
                existing.Description,  decimal.Round(existing.Sum, 2),  existing.Person?.Name ?? "null");
            return operationResultFactory.SuccessDeleted($"{EntityName}: '{existing.Description}'");
        }
        catch (DbUpdateException dbUpdateException)
        {
            logger.LogError(dbUpdateException, "An exception occurred while deleting transaction with id: {Id}", id);
            return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred while deleting transaction with id: {Id}", id);
            return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
        }
    }
}
