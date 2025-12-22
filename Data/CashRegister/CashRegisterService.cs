using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.CashRegister
{
    public class CashRegisterService(CashDataContext context, ILogger<CashRegisterService> logger) : ICashRegisterService
    {
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

        public async Task AddCashRegister(CashRegisterModel cashRegisterModel)
        {
            try
            {
                await context.CashRegisters.AddAsync(cashRegisterModel);
                await context.SaveChangesAsync();
                logger.LogInformation("Cash register added: {@CashRegister}", cashRegisterModel.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured while adding cash register entry do database.");
            }
        }

        public async Task UpdateCashRegister(CashRegisterModel cashRegisterModel)
        {
            try
            {
                context.CashRegisters.Update(cashRegisterModel);
                await context.SaveChangesAsync();
                logger.LogInformation("Cash register updated: {@CashRegister}", cashRegisterModel.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured during update of cash register: {@CashRegister}"
                    , cashRegisterModel);
            }
        }

        public async Task<bool> DeleteCashRegister(int id)
        {
            try
            {
                var cashRegister = await context.CashRegisters.FindAsync(id);
                if (cashRegister is null)
                {
                    logger.LogError("Cashregister not found: {Id}", id);
                    return false;
                }

                context.CashRegisters.Remove(cashRegister);
                await context.SaveChangesAsync();
                logger.LogInformation("Cash register deleted: {@CashRegister}", cashRegister.Name);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured during delete of cash register: {Id}", id);
                return false;
            }
        }
    }
}
