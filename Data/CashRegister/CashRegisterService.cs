using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.CashRegister
{
    public class CashRegisterService(CashDataContext context, ILogger<CashRegisterService> logger)
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
                logger.LogInformation("Cashregister found: {@CashRegister}", cashRegister);
                return cashRegister;
            }
            logger.LogError($"Cashregister not found: {cashRegister}");
            return null;
        }

        public async Task<CashRegisterModel?> GetFirstEntry()
        {
            var cashRegister = await context.CashRegisters.FirstOrDefaultAsync();
            if (cashRegister is not null)
            {
                logger.LogInformation("First cashregister found: {@CashRegister}", cashRegister);
                return cashRegister;
            }
            logger.LogError($"No cahsregister data found: {cashRegister}");
            return null;
        }

        public async Task AddCashRegister(CashRegisterModel cashRegisterModel)
        {
            try
            {
                await context.CashRegisters.AddAsync(cashRegisterModel);
                await context.SaveChangesAsync();
                logger.LogInformation("Cashregister added: {@CashRegister}", cashRegisterModel);
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
                logger.LogInformation("Cashregister updated: {@CashRegister}", cashRegisterModel);
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
                logger.LogInformation("Cashregister deleted: {@CashRegister}", cashRegister);
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
