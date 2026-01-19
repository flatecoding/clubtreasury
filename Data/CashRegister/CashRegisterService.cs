using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TTCCashRegister.Data.Notification;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.CashRegister
{
    public class CashRegisterService(
        CashDataContext context,
        ILogger<CashRegisterService> logger,
        IOperationResultFactory operationResultFactory,
        IStringLocalizer<Translation> localizer)
        : ICashRegisterService
    {
        private string EntityName => localizer["CashRegister"];

        public async Task<List<CashRegisterModel>> GetAllCashRegisters()
        {
            return await context.CashRegisters
                .Include(t => t.Transactions)
                .ToListAsync();
        }

        public async Task<CashRegisterModel?> GetCashRegisterById(int id)
        {
            var cashRegister = await context.CashRegisters.FindAsync(id);
            if (cashRegister is not null)
            {
                logger.LogInformation("Cash register found: {@CashRegister}", cashRegister.Name);
                return cashRegister;
            }

            logger.LogError($"Cash register not found with id '{id}'");
            return null;
        }

        public async Task<CashRegisterModel?> GetFirstEntry()
        {
            var cashRegister = await context.CashRegisters.FirstOrDefaultAsync();
            if (cashRegister is not null)
            {
                logger.LogInformation("First cash register found: {@CashRegister}", cashRegister.Name);
                return cashRegister;
            }

            logger.LogError($"No cach register data available");
            return null;
        }

        public async Task<IOperationResult> AddCashRegister(CashRegisterModel cashRegisterModel)
        {
            try
            {
                await context.CashRegisters.AddAsync(cashRegisterModel);
                await context.SaveChangesAsync();
                logger.LogInformation("Cash register added: {@CashRegister}", cashRegisterModel.Name);
                return operationResultFactory.SuccessAdded($"{EntityName} - '{cashRegisterModel.Name}'", cashRegisterModel.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while adding cash register entry to database.");
                return operationResultFactory.FailedToAdd(EntityName, ex.Message);
            }
        }

        public async Task<IOperationResult> UpdateCashRegister(CashRegisterModel cashRegisterModel)
        {
            try
            {
                context.CashRegisters.Update(cashRegisterModel);
                await context.SaveChangesAsync();
                logger.LogInformation("Cash register updated: {@CashRegister}", cashRegisterModel.Name);
                return operationResultFactory.SuccessUpdated($"{EntityName}: '{cashRegisterModel.Name}'", cashRegisterModel.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during update of cash register: {@CashRegister}",
                    cashRegisterModel);
                return operationResultFactory.FailedToUpdate(EntityName, ex.Message);
            }
        }


        public async Task<IOperationResult> DeleteCashRegisterAsync(int id)
        {
            try
            {
                var cashRegister = await context.CashRegisters.FindAsync(id);
                if (cashRegister is null)
                {
                    logger.LogWarning("Cash register with Id {CashRegisterId} not found.", id);
                    return operationResultFactory.NotFound(EntityName, id);
                }

                context.CashRegisters.Remove(cashRegister);
                await context.SaveChangesAsync();
                logger.LogInformation("Cash register deleted: {@CashRegister}", cashRegister.Name);
                return operationResultFactory.SuccessDeleted(EntityName, id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during delete of cash register: {Id}", id);
                return operationResultFactory.FailedToDelete(EntityName, ex.Message);
            }
        }
    }
}
