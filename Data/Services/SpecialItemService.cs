using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.Models;

namespace TTCCashRegister.Data.Services
{
    public class SpecialItemService
    {
        private readonly CashDataContext _context;

        public SpecialItemService(CashDataContext context)
        {
            _context = context;
        }

        public async Task<List<SpecialItem>> GetAllSonderposten()
        {
            return _context.SpecialItems is not null ? await _context.SpecialItems
                                                            .ToListAsync() : new List<SpecialItem>();
        }

        public async Task<SpecialItem?> GetSonderpostenById(int id)
        {
            return await _context.SpecialItems.FindAsync(id);
        }

        public async Task<bool> AddSonderposten(SpecialItem sonderposten)
        {
            try
            {
                await _context.SpecialItems.AddAsync(sonderposten);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<bool> UpdateSonderposten(SpecialItem sonderposten)
        {
            try
            {
                _context.SpecialItems.Update(sonderposten);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<bool> DeleteSonderposten(int id)
        {
            try
            {
                var sonderposten = await _context.SpecialItems.FindAsync(id);
                if (sonderposten == null)
                {
                    return false;
                }

                _context.SpecialItems.Remove(sonderposten);
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
