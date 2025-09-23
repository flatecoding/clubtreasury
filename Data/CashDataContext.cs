using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.Accounts;
using TTCCashRegister.Data.Transaction;
using TTCCashRegister.Data.UnitDetail;
using TTCCashRegister.Data.CashRegister;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;
using TTCCashRegister.Data.Person;
using TTCCashRegister.Data.SpecialItem;
using TTCCashRegister.Data.SubTransaction;

namespace TTCCashRegister.Data
{
    public class CashDataContext(DbContextOptions<CashDataContext> options) : DbContext(options)
    {
        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<CostCenterModel> CostCenters { get; set; }
        public DbSet<CashRegisterModel> CashRegisters { get; set; }
        public DbSet<UnitDetailsModel> UnitDetails { get; set; }
        public DbSet<SpecialItemModel> SpecialItems { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<PersonModel> Persons { get; set; }
        public DbSet<SubTransactionModel> SubTransactions { get; set; }
        public DbSet<AccountsModel> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CostUnit ↔ BasicUnit (1:n)
            modelBuilder.Entity<CategoryModel>()
                .HasOne(b => b.CostCenter)
                .WithMany(c => c.Categories) 
                .HasForeignKey(b => b.CostCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Accounts ↔ BasicUnit (n:1)
            modelBuilder.Entity<AccountsModel>()
                .HasOne(a => a.Category)
                .WithMany(b => b.Accounts)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Accounts ↔ UnitDetails (n:1)
            modelBuilder.Entity<AccountsModel>()
                .HasOne(a => a.UnitDetails)
                .WithMany(ud => ud.Accounts)
                .HasForeignKey(a => a.UnitDetailsId)
                .OnDelete(DeleteBehavior.Restrict);

            // SubTransaction ↔ Person (n:1)
            modelBuilder.Entity<SubTransactionModel>()
                .HasOne(st => st.Person)
                .WithMany()
                .HasForeignKey(st => st.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            // Optional: Unique Constraints für Documentnumber
            modelBuilder.Entity<TransactionModel>()
                .HasIndex(t => t.Documentnumber)
                .IsUnique();

            // Optional: Decimal Precision
            modelBuilder.Entity<TransactionModel>()
                .Property(t => t.Sum)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<TransactionModel>()
                .Property(t => t.AccountMovement)
                .HasColumnType("decimal(10,2)");
        }

    }
}
