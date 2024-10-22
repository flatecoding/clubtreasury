using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.Models;

namespace TTCCashRegister.Data.Services
{
    public class CashRegisterService
    {
        private readonly CashDataContext _context;

        public CashRegisterService(CashDataContext context)
        {
            _context = context;
        }

        public async Task<List<CashRegister>> GetAllCashRegisters()
        {
            return await _context.CashRegisters.ToListAsync();
        }

        public async Task<CashRegister?> GetCashRegisterById(int id)
        {
            return await _context.CashRegisters.FindAsync(id);
        }

        public async Task<CashRegister?> GetFirstEntry()
        {
            return await _context.CashRegisters.FirstOrDefaultAsync();
        }

        public async Task<bool> AddCashRegister(CashRegister cashRegister)
        {
            try
            {
                await _context.CashRegisters.AddAsync(cashRegister);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<bool> UpdateCashRegister(CashRegister cashRegister)
        {
            try
            {
                _context.CashRegisters.Update(cashRegister);
                await _context.SaveChangesAsync();
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
                var cashRegister = await _context.CashRegisters.FindAsync(id);
                if (cashRegister == null)
                {
                    return false;
                }

                _context.CashRegisters.Remove(cashRegister);
                await _context.SaveChangesAsync();
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
