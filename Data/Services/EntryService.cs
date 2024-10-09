using System.Data.Entity;
using TTCCashRegister.Data.Models;

namespace TTCCashRegister.Data.Services
{
    public class EntryService
    {
        private readonly CashDataContext _context;

        public EntryService(CashDataContext context)
        {
            _context = context;
        }

        public async Task<List<Entry>?> GetAllSectors()
        {
            return _context.Entries != null ? await _context.Entries
                                                             .OrderByDescending(x => x.Id)
                                                             .ToListAsync(): null;
        }

        public async Task<bool> AddSector(Entry entry)
        {
            try
            {
                await _context.Entries.AddAsync(entry);
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
