using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.Models;

namespace TTCCashRegister.Data
{
    public class CashDataContext: DbContext
    {
        public DbSet<Entry> Entries { get; set; }
        public DbSet<BusinessSector> BusinessSectors { get; set; }
        public DbSet<CashRegister> CashRegister { get; set; } = default!;

        public CashDataContext(DbContextOptions<CashDataContext> options)
           : base(options)
        {
        }

    }

}
