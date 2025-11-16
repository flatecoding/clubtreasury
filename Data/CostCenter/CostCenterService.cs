using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.CostCenter
{
    public class CostCenterService(CashDataContext context, ILogger<CostCenterService> logger)
    {
        private readonly CashDataContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<List<CostCenterModel>> GetAllCostCentersAsync()
        {
            return await _context.CostCenters
                .Include(c => c.Allocations)
                    .ThenInclude(a => a.Category)
                .Include(c => c.Allocations)
                    .ThenInclude(a => a.ItemDetail)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }
        
        public async Task<CostCenterModel?> GetCostCenterByIdAsync(int id)
        {
            return await _context.CostCenters
                .Include(c => c.Allocations)
                    .ThenInclude(a => a.Category)
                .Include(c => c.Allocations)
                    .ThenInclude(a => a.ItemDetail)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> AddCostCenterAsync(CostCenterModel costCenter)
        {
            try
            {
                await _context.CostCenters.AddAsync(costCenter);
                await _context.SaveChangesAsync();
                logger.LogInformation("Cost center added: {@costCenter}", costCenter.CostUnitName);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured during add cost center: {@costCenter}", costCenter.CostUnitName);
                return false;
            }
        }

        public async Task<bool> UpdateCostCenterAsync(CostCenterModel costCenter)
        {
            try
            {
                _context.CostCenters.Update(costCenter);
                await _context.SaveChangesAsync();
                logger.LogInformation("Cost center updated: {@costCenter}", costCenter.CostUnitName);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured during update of cost center: {@costCenter}", costCenter.CostUnitName);
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
                logger.LogInformation("Cost center deleted: {@costUnit}", costUnit.CostUnitName);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured during delete of costunit with Id: {Id}", id );
                return false;
            }
        }
    }
}
