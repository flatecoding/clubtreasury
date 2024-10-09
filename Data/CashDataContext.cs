using System.Data;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Hosting;
using TTCCashRegister.Data.Models;

namespace TTCCashRegister.Data
{
    public class CashDataContext: DbContext
    {
        static readonly string connectionString = "Server=192.168.10.51; User ID=dev; Password=Malcom_1%9; Database=TTCCash";

        public DbSet<Entry> Entries { get; set; }
        public DbSet<BusinessSector> BusinessSectors { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }

    }

}
