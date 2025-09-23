using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.UnitDetail
{
    public class UnitDetailService
    {
        private readonly CashDataContext _context;

        public UnitDetailService(CashDataContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<UnitDetailsModel>> GetAllDetailsAsync()
        {
            return await _context.UnitDetails
                .Include(ud => ud.Accounts)
                .ThenInclude(a => a.BasicUnit)
                .Include(ud => ud.Accounts)
                .ThenInclude(a => a.CostCenter)
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }

        public async Task<UnitDetailsModel?> GetUnitDetailsByIdAsync(int id)
        {
            return await _context.UnitDetails
                .Include(ud => ud.Accounts)
                .ThenInclude(a => a.BasicUnit)
                .Include(ud => ud.Accounts)
                .ThenInclude(a => a.CostCenter)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<UnitDetailsModel>> GetUnitDetailsByBasicUnitIdAsync(int basicUnitId)
        {
            return await _context.UnitDetails
                .Where(u => u.Accounts.Any(a => a.BasicUnitId == basicUnitId))
                .ToListAsync();
        }

        public async Task<UnitDetailsModel?> AddUnitDetailAsync(UnitDetailsModel unitDetail)
        {
            try
            {
                await _context.UnitDetails.AddAsync(unitDetail);
                await _context.SaveChangesAsync();
                return unitDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return null;
            }
        }

        public async Task<bool> UpdateUnitDetailsAsync(UnitDetailsModel unitDetail)
        {
            try
            {
                _context.UnitDetails.Update(unitDetail);
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
                var unitDetail = await _context.UnitDetails.FindAsync(id);
                if (unitDetail == null) return false;
                _context.UnitDetails.Remove(unitDetail);
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
