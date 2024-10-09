using MySqlConnector;
using System.Data.Entity;
using TTCCashRegister.Data.Models;

namespace TTCCashRegister.Data.Services
{
    public class BusinessSectorService
    {
        private readonly CashDataContext _context;

        public BusinessSectorService(CashDataContext context)
        {
            _context = context;
        }

        public async Task<List<BusinessSector>?> GetAllSectors()
        {
            return _context.BusinessSectors != null ? await _context.BusinessSectors
                                                             .OrderByDescending(x => x.Id)
                                                             .ToListAsync() : null;
        }

        public async Task<bool> AddSector(BusinessSector businessSector)
        {
            try
            {
                await _context.BusinessSectors.AddAsync(businessSector);
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
