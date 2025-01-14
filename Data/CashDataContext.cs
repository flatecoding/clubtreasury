using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.CostUnit;
using TTCCashRegister.Data.Transaction;
using TTCCashRegister.Data.UnitDetail;
using TTCCashRegister.Data.BasicUnit;
using TTCCashRegister.Data.CashRegister;
using TTCCashRegister.Data.SpecialItem;

namespace TTCCashRegister.Data
{
    public class CashDataContext(DbContextOptions<CashDataContext> options) : DbContext(options)
    {
        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<CostUnitModel> CostUnits { get; set; }
        public DbSet<CashRegisterModel> CashRegisters { get; set; }
        public DbSet<UnitDetailsModel> UnitDetails { get; set; }
        public DbSet<SpecialItemModel> SpecialItems { get; set; }
        public DbSet<BasicUnitModel> BasicUnits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<BasicUnitModel>()
                .HasOne(b => b.CostUnit)
                .WithMany(c => c.BasicUnitDetails)
                .HasForeignKey(b => b.CostUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CostUnitModel>()
                .HasMany(c => c.BasicUnitDetails);


        }
    }
}
