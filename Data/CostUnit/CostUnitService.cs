using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.CostUnit
{
    public class CostUnitService
    {
        private readonly CashDataContext _context;

        public CostUnitService(CashDataContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        public async Task<List<CostUnitModel>> GetAllUnitsAsync()
        {
            return await _context.CostUnits
                .Include(c => c.Accounts)
                    .ThenInclude(a => a.BasicUnit)
                .Include(c => c.Accounts)
                    .ThenInclude(a => a.UnitDetails)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }
        
        public async Task<CostUnitModel?> GetCostUnitByIdAsync(int id)
        {
            return await _context.CostUnits
                .Include(c => c.Accounts)
                    .ThenInclude(a => a.BasicUnit)
                .Include(c => c.Accounts)
                    .ThenInclude(a => a.UnitDetails)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> AddCostUnitAsync(CostUnitModel costUnit)
        {
            try
            {
                await _context.CostUnits.AddAsync(costUnit);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<bool> UpdateCostUnitAsync(CostUnitModel costUnit)
        {
            try
            {
                _context.CostUnits.Update(costUnit);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<bool> DeleteCostUnitAsync(int id)
        {
            try
            {
                var costUnit = await _context.CostUnits.FindAsync(id);
                if (costUnit == null) return false;
                _context.CostUnits.Remove(costUnit);
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
