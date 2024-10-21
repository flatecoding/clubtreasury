using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.Models;

namespace TTCCashRegister.Data
{
    public class CashDataContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CostUnit> CostUnits { get; set; }
        public DbSet<CashRegister> CashRegisters { get; set; } = default!;
        public DbSet<UnitDetails> UnitDetails { get; set; }
        public DbSet<SpecialItem> SpecialItems { get; set; }
        public DbSet<BasicUnit> BasicUnits { get; set; }

        public CashDataContext(DbContextOptions<CashDataContext> options)
           : base(options)
        {
        }

    }

}
