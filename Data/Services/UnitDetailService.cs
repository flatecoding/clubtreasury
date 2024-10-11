using TTCCashRegister.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TTCCashRegister.Data.Services
{
    public class UnitDetailService
    {
        private readonly CashDataContext _context;

        public UnitDetailService(CashDataContext context)
        {
            _context = context;
        }

        public async Task<List<UnitDetails>?> GetAllPositions()
        {
            return _context.UnitDetails is not null ? await _context.UnitDetails
                                                                .Include(x =>x.CostUnit)
                                                                .OrderByDescending(c => c.Id)
                                                                .ToListAsync() : new List<UnitDetails>();
        }

        public async Task<bool> AddUnitDetail(UnitDetails detail)
        {
            try
            {
                await _context.UnitDetails.AddAsync(detail);
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
