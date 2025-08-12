using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.BasicUnit
{
    public class BasicUnitService
    {
        private readonly CashDataContext _context;

        public BasicUnitService(CashDataContext context)
        {
            _context = context;
        }

        // Create
        public async Task<bool> AddBasicUnitAsync(BasicUnitModel unit)
        {
            _context.BasicUnits.Add(unit);
            await _context.SaveChangesAsync();
            return true;
        }

        // Read
        public async Task<BasicUnitModel?> GetBasicUnitByIdAsync(int id)
        {
            return await _context.BasicUnits
                                 .Include(b => b.CostUnitDetails)
                                 .FirstOrDefaultAsync(b => b.Id == id);
        }
        
        public async Task<IEnumerable<BasicUnitModel>?> GetBasicUnitsByCostUnitIdAsync(int costunitId)
        {
            try
            {
                return await _context.BasicUnits
                    .Where(x => x.CostUnit != null && x.CostUnit.Id == costunitId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return null;
            }
        }

        public async Task<IEnumerable<BasicUnitModel>> GetAllBasicUnitsAsync()
        {
            return await _context.BasicUnits
                                 .Include(b => b.CostUnitDetails)
                                 .ToListAsync();
        }

        // Update
        public async Task UpdateBasicUnitAsync(BasicUnitModel unit)
        {
            _context.BasicUnits.Update(unit);
            await _context.SaveChangesAsync();
        }

        // Delete
        public async Task<bool> DeleteBasicUnitAsync(int id)
        {
            var unit = await _context.BasicUnits.FindAsync(id);
            if (unit is null) return false;
            _context.BasicUnits.Remove(unit);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
