using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.Transaction;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.ItemDetail;
using ClubTreasury.Data.Person;
using ClubTreasury.Data.SpecialItem;
using ClubTreasury.Data.TransactionDetails;

namespace ClubTreasury.Data
{
    public class CashDataContext(DbContextOptions<CashDataContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<CostCenterModel> CostCenters { get; set; }
        public DbSet<CashRegisterModel> CashRegisters { get; set; }
        public DbSet<ItemDetailModel> ItemDetails { get; set; }
        public DbSet<SpecialItemModel> SpecialItems { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<PersonModel> Persons { get; set; }
        public DbSet<TransactionDetailsModel> TransactionDetails { get; set; }
        public DbSet<AllocationModel> Allocations { get; set; }
        public DbSet<CashRegisterLogoModel> CashRegisterLogos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<AllocationModel>()
                .HasOne(a => a.CostCenter)
                .WithMany(c => c.Allocations)
                .HasForeignKey(a => a.CostCenterId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<AllocationModel>()
                .HasOne(a => a.Category)
                .WithMany(b => b.Allocations)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<AllocationModel>()
                .HasOne(a => a.ItemDetail)
                .WithMany(ud => ud.Allocations)
                .HasForeignKey(a => a.ItemDetailId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<TransactionDetailsModel>()
                .HasOne(st => st.Person)
                .WithMany()
                .HasForeignKey(st => st.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<TransactionModel>()
                .HasIndex(t => new { t.Documentnumber, t.CashRegisterId })
                .IsUnique();
            
            builder.Entity<TransactionModel>()
                .Property(t => t.Sum)
                .HasColumnType("decimal(10,2)");

            builder.Entity<TransactionModel>()
                .Property(t => t.AccountMovement)
                .HasColumnType("decimal(10,2)");

            builder.Entity<TransactionModel>()
                .HasOne(t => t.Allocation)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AllocationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<AllocationModel>()
                .HasIndex(a => new { a.CostCenterId, a.CategoryId, a.ItemDetailId })
                .IsUnique();

            builder.Entity<CashRegisterModel>()
                .HasOne(cr => cr.Treasurer)
                .WithMany()
                .HasForeignKey(cr => cr.TreasurerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CashRegisterModel>()
                .HasOne(cr => cr.Logo)
                .WithOne(l => l.CashRegister)
                .HasForeignKey<CashRegisterLogoModel>(l => l.CashRegisterId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
