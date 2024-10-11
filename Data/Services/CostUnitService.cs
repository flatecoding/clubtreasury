using MySqlConnector;
using TTCCashRegister.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.Services
{
    public class CostUnitService
    {
        private readonly CashDataContext _context;

        public CostUnitService(CashDataContext context)
        {
            _context = context;
        }

        public async Task<List<CostUnit>?> GetAllUnits()
        {
            return _context.CostUnits is not null ? await _context.CostUnits
                                                             .Include(c => c.CostUnitDetails)
                                                             .OrderBy(x => x.Id)
                                                             .ToListAsync() : new List<CostUnit>();
        }

        public async Task<bool> AddUnit(CostUnit businessSector)
        {
            try
            {
                await _context.CostUnits.AddAsync(businessSector);
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
