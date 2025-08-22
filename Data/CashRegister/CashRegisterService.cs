using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.CashRegister
{
    public class CashRegisterService(CashDataContext context)
    {
        public async Task<List<CashRegisterModel>> GetAllCashRegisters()
        {
            return await context.CashRegisters
                .Include(t => t.Transactions)
                .ToListAsync();
        }

        public async Task<CashRegisterModel?> GetCashRegisterById(int id)
        {
            return await context.CashRegisters.FindAsync(id);
        }

        public async Task<CashRegisterModel?> GetFirstEntry()
        {
            return await context.CashRegisters.FirstOrDefaultAsync();
        }

        public async Task<bool> AddCashRegister(CashRegisterModel cashRegisterModel)
        {
            try
            {
                await context.CashRegisters.AddAsync(cashRegisterModel);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<bool> UpdateCashRegister(CashRegisterModel cashRegisterModel)
        {
            try
            {
                context.CashRegisters.Update(cashRegisterModel);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<bool> DeleteCashRegister(int id)
        {
            try
            {
                var cashRegister = await context.CashRegisters.FindAsync(id);
                if (cashRegister == null)
                {
                    return false;
                }

                context.CashRegisters.Remove(cashRegister);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }
    }
}
