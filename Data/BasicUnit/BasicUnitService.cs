using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.BasicUnit
{
    public class BasicUnitService
    {
        private readonly CashDataContext _context;

        public BasicUnitService(CashDataContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        public async Task<List<BasicUnitModel>> GetAllBasicUnitsAsync()
        {
            return await _context.BasicUnits
                .Include(b => b.Accounts)
                    .ThenInclude(a => a.CostCenter)
                .Include(b => b.Accounts)
                    .ThenInclude(a => a.UnitDetails)
                .ToListAsync();
        }

        public async Task<BasicUnitModel?> GetBasicUnitByIdAsync(int id)
        {
            return await _context.BasicUnits
                .Include(b => b.Accounts)
                    .ThenInclude(a => a.CostCenter)
                .Include(b => b.Accounts)
                    .ThenInclude(a => a.UnitDetails)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<BasicUnitModel>> GetBasicUnitsByCostUnitIdAsync(int costUnitId)
        {
            return await _context.BasicUnits
                .Where(b => b.Accounts.Any(a => a.CostCenterId == costUnitId))
                .ToListAsync();
        }

        public async Task AddBasicUnitAsync(BasicUnitModel unit)
        {
            _context.BasicUnits.Add(unit);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBasicUnitAsync(BasicUnitModel unit)
        {
            _context.BasicUnits.Update(unit);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteBasicUnitAsync(int id)
        {
            var unit = await _context.BasicUnits.FindAsync(id);
            if (unit == null) return false;
            _context.BasicUnits.Remove(unit);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
