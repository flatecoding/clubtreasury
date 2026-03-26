using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.CashRegister
{
    public class CashRegisterService(
        CashDataContext context,
        ILogger<CashRegisterService> logger,
        IResultFactory operationResultFactory,
        IStringLocalizer<Translation> localizer)
        : ICashRegisterService
    {
        private string EntityName => localizer["CashRegister"];

        public async Task<List<CashRegisterModel>> GetAllCashRegistersAsync(CancellationToken ct = default)
        {
            return await context.CashRegisters
                .ToListAsync(ct);
        }

        public async Task<Dictionary<int, decimal>> GetCashRegisterBalancesAsync(CancellationToken ct = default)
        {
            return await context.Transactions
                .GroupBy(t => t.CashRegisterId)
                .Select(g => new { CashRegisterId = g.Key, Balance = g.Sum(t => t.AccountMovement) })
                .ToDictionaryAsync(x => x.CashRegisterId, x => x.Balance, ct);
        }

        public async Task<CashRegisterModel?> GetCashRegisterByIdAsync(int id, CancellationToken ct = default)
        {
            var cashRegister = await context.CashRegisters.FindAsync([id], ct);
            if (cashRegister is not null)
            {
                logger.LogInformation("Cash register found: {@CashRegister}", cashRegister.Name);
                return cashRegister;
            }

            logger.LogError("Cash register not found with id '{CashRegisterId}'", id);
            return null;
        }

        public async Task<CashRegisterModel?> GetFirstCashRegisterAsync(CancellationToken ct = default)
        {
            var cashRegister = await context.CashRegisters.FirstOrDefaultAsync(ct);
            if (cashRegister is not null)
            {
                logger.LogInformation("First cash register found: {@CashRegister}", cashRegister.Name);
                return cashRegister;
            }

            logger.LogError("No cash register data available");
            return null;
        }

        public async Task<Result> AddCashRegisterAsync(CashRegisterModel cashRegisterModel, CancellationToken ct = default)
        {
            try
            {
                await context.CashRegisters.AddAsync(cashRegisterModel, ct);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Cash register added: {@CashRegister}", cashRegisterModel.Name);
                return operationResultFactory.SuccessAdded($"{EntityName}: '{cashRegisterModel.Name}'", cashRegisterModel.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while adding cash register entry to database.");
                return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
            }
        }

        public async Task<Result> UpdateCashRegisterAsync(CashRegisterModel cashRegisterModel, CancellationToken ct = default)
        {
            try
            {
                context.CashRegisters.Update(cashRegisterModel);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Cash register updated: {@CashRegister}", cashRegisterModel.Name);
                return operationResultFactory.SuccessUpdated($"{EntityName}: '{cashRegisterModel.Name}'", cashRegisterModel.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during update of cash register: {@CashRegister}",
                    cashRegisterModel);
                return operationResultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
            }
        }


        public async Task<Result> DeleteCashRegisterAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var cashRegister = await context.CashRegisters.FindAsync([id], ct);
                if (cashRegister is null)
                {
                    logger.LogWarning("Cash register with Id {CashRegisterId} not found.", id);
                    return operationResultFactory.NotFound(EntityName, id);
                }

                var transactions = await context.Transactions
                    .Include(t => t.TransactionDetails)
                    .Where(t => t.CashRegisterId == id)
                    .ToListAsync(ct);

                if (transactions.Count > 0)
                {
                    context.TransactionDetails.RemoveRange(transactions.SelectMany(t => t.TransactionDetails));
                    context.Transactions.RemoveRange(transactions);
                    logger.LogInformation(
                        "Deleting {Count} transactions for cash register {CashRegisterId}",
                        transactions.Count, id);
                }

                context.CashRegisters.Remove(cashRegister);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Cash register deleted: {@CashRegister}", cashRegister.Name);
                return operationResultFactory.SuccessDeleted(EntityName, id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during delete of cash register: {Id}", id);
                return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
            }
        }

    }
}