using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.UnitDetail
{
    public class UnitDetailService
    {
        private readonly CashDataContext _context;

        public UnitDetailService(CashDataContext context)
        {
            _context = context;
        }

        public async Task<List<UnitDetailsModel>?> GetAllDetailsAsync()
        {
            return await _context.UnitDetails
                .Include(x => x.BasicUnits)
                .OrderByDescending<UnitDetailsModel, int>(c => c.Id)
                .ToListAsync();
        }

        public async Task<bool> AddUnitDetailAsync(UnitDetailsModel detailModel)
        {
            try
            {
                await _context.UnitDetails.AddAsync(detailModel);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<UnitDetailsModel?> GetUnitDetailsByIdAsync(int id)
        {
            try
            {
                return await _context.UnitDetails
                                     .Include(x => x.BasicUnits)
                                     .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return null;
            }
        }
        
        public async Task<List<UnitDetailsModel>?> GetUnitDetailsByBasicUnitAsync(int id)
        {
            try
            {
                return await _context.UnitDetails
                    .Where(ud => ud.BasicUnits.Any(bu => bu.Id == id))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return null;
            }
        }

        public async Task<bool> UpdateUnitDetailsAsync(UnitDetailsModel detailModel)
        {
            try
            {
                _context.UnitDetails.Update(detailModel);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }

        public async Task<bool> DeleteUnitDetailsAsync(int id)
        {
            try
            {
                var detail = await _context.UnitDetails.FindAsync(id);
                if (detail == null)
                {
                    return false;
                }

                _context.UnitDetails.Remove(detail);
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
