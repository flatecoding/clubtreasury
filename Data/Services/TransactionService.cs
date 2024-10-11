using TTCCashRegister.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.Services
{
    public class TransactionService
    {
        private readonly CashDataContext _context;

        public TransactionService(CashDataContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>?> GetAllSectors()
        {
            return _context.Transactions != null ? await _context.Transactions
                                                             .OrderByDescending(x => x.Id)
                                                             .ToListAsync(): null;
        }

        public async Task<bool> AddSector(Transaction entry)
        {
            try
            {
                await _context.Transactions.AddAsync(entry);
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
