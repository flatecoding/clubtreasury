using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.CostUnit
{
    public class CostUnitService
    {
        private readonly CashDataContext _context;

        public CostUnitService(CashDataContext context)
        {
            _context = context;
        }

        public async Task<List<CostUnitModel>> GetAllUnitsAsync()
        {
            try
            {
                if (_context.CostUnits == null)
                {
                    throw new Exception("Cost units in DbContext is null");
                }

                var result = await _context.CostUnits
                                           .Include(c => c.BasicUnitDetails)
                                           .OrderBy(x => x.Id)
                                           .ToListAsync();

                if (result == null)
                {
                    throw new Exception("Ergebnis ist null");
                }

                return result;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Datenbankfehler: {dbEx.Message}");
                throw; // Rethrow um den Stacktrace zu erhalten
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Allgemeiner Fehler: {ex.Message}");
                throw; // Rethrow um den Stacktrace zu erhalten
            }
        }

        public async Task<bool> AddCostUnit(CostUnitModel costUnitModel)
        {
            try
            {
                await _context.CostUnits.AddAsync(costUnitModel);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<CostUnitModel?> GetCostUnitByIdAsync(int id)
        {
            try
            {
                return await _context.CostUnits
                                     .Include(c => c.BasicUnitDetails)
                                     .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return null;
            }
        }

        public async Task<bool> UpdateCostUnitAsync(CostUnitModel costUnitModel)
        {
            try
            {
                _context.CostUnits.Update(costUnitModel);
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
                if (costUnit == null)
                {
                    return false;
                }

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
