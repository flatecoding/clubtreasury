using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.CostCenter
{
    public class CostCenterService(CashDataContext context)
    {
        private readonly CashDataContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<List<CostCenterModel>> GetAllCostCentersAsync()
        {
            return await _context.CostCenters
                .Include(c => c.Accounts)
                    .ThenInclude(a => a.Category)
                .Include(c => c.Accounts)
                    .ThenInclude(a => a.UnitDetails)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }
        
        public async Task<CostCenterModel?> GetCostCenterByIdAsync(int id)
        {
            return await _context.CostCenters
                .Include(c => c.Accounts)
                    .ThenInclude(a => a.Category)
                .Include(c => c.Accounts)
                    .ThenInclude(a => a.UnitDetails)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> AddCostCenterAsync(CostCenterModel costCenter)
        {
            try
            {
                await _context.CostCenters.AddAsync(costCenter);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<bool> UpdateCostCenterAsync(CostCenterModel costCenter)
        {
            try
            {
                _context.CostCenters.Update(costCenter);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<bool> DeleteCostCenterAsync(int id)
        {
            try
            {
                var costUnit = await _context.CostCenters.FindAsync(id);
                if (costUnit == null) return false;
                _context.CostCenters.Remove(costUnit);
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
