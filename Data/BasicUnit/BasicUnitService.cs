using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.BasicUnit
{
    public class BasicUnitService(CashDataContext context)
    {
        // Create
        public async Task AddBasicUnitAsync(BasicUnitModel unit)
        {
            context.BasicUnits.Add(unit);
            await context.SaveChangesAsync();
        }

        // Read
        public async Task<BasicUnitModel?> GetBasicUnitByIdAsync(int id)
        {
            return await context.BasicUnits
                                 .Include(b => b.CostUnitDetails)
                                 .FirstOrDefaultAsync(b => b.Id == id);
        }
        
        public async Task<IEnumerable<BasicUnitModel>?> GetBasicUnitsByCostUnitIdAsync(int costunitId)
        {
            try
            {
                return await context.BasicUnits
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
            return await context.BasicUnits
                                 .Include(b => b.CostUnitDetails)
                                 .ToListAsync();
        }

        // Update
        public async Task UpdateBasicUnitAsync(BasicUnitModel unit)
        {
            context.BasicUnits.Update(unit);
            await context.SaveChangesAsync();
        }

        // Delete
        public async Task<bool> DeleteBasicUnitAsync(int id)
        {
            var unit = await context.BasicUnits.FindAsync(id);
            if (unit is null) return false;
            context.BasicUnits.Remove(unit);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
