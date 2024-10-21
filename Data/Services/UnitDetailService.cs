using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.Models;

namespace TTCCashRegister.Data.Services
{
    public class UnitDetailService
    {
        private readonly CashDataContext _context;

        public UnitDetailService(CashDataContext context)
        {
            _context = context;
        }

        public async Task<List<UnitDetails>?> GetAllDetailsAsync()
        {
            return _context.UnitDetails is not null ? await _context.UnitDetails
                                                                .Include(x => x.BasicUnit)
                                                                .OrderByDescending(c => c.Id)
                                                                .ToListAsync() : new List<UnitDetails>();
        }

        public async Task<bool> AddUnitDetailAsync(UnitDetails detail)
        {
            try
            {
                await _context.UnitDetails.AddAsync(detail);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<UnitDetails?> GetUnitDetailsByIdAsync(int id)
        {
            try
            {
                return await _context.UnitDetails
                                     .Include(x => x.BasicUnit)
                                     .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return null;
            }
        }

        public async Task<bool> UpdateUnitDetailsAsync(UnitDetails detail)
        {
            try
            {
                _context.UnitDetails.Update(detail);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<bool> DeleteUnitDetailsAsync(int id)
        {
            try
            {
                var detail = await _context.UnitDetails.FindAsync(id);
                if (detail == null)
                {
                    return false;
                }

                _context.UnitDetails.Remove(detail);
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
