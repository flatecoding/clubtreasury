using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.Models;

namespace TTCCashRegister.Data.Services
{
    public class BasicUnitService
    {
        private readonly CashDataContext _context;

        public BasicUnitService(CashDataContext context)
        {
            _context = context;
        }

        // Create
        public async Task<bool> AddBasicUnitAsync(BasicUnit unit)
        {
            _context.BasicUnits.Add(unit);
            await _context.SaveChangesAsync();
            return true;
        }

        // Read
        public async Task<BasicUnit?> GetBasicUnitByIdAsync(int id)
        {
            return await _context.BasicUnits
                                 .Include(b => b.CostUnitDetails)
                                 .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<BasicUnit>> GetAllBasicUnitsAsync()
        {
            return await _context.BasicUnits
                                 .Include(b => b.CostUnitDetails)
                                 .ToListAsync();
        }

        // Update
        public async Task UpdateBasicUnitAsync(BasicUnit unit)
        {
            _context.BasicUnits.Update(unit);
            await _context.SaveChangesAsync();
        }

        // Delete
        public async Task<bool> DeleteBasicUnitAsync(int id)
        {
            var unit = await _context.BasicUnits.FindAsync(id);
            if (unit != null)
            {
                _context.BasicUnits.Remove(unit);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
