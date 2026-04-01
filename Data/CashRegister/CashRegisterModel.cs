using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClubTreasury.Data.Person;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Data.CashRegister
{
    public class CashRegisterModel
    {
        [Key]
        public int Id { get; init; }
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        [Range(1, 12)]
        public int FiscalYearStartMonth { get; set; } = 7;
        public int? TreasurerId { get; set; }
        public PersonModel? Treasurer { get; set; }
        [NotMapped]
        public decimal CurrentBalance => Transactions?.Sum(t => t.AccountMovement) ?? 0m;
        public ICollection<TransactionModel> Transactions { get; init; } = new List<TransactionModel>();
        public CashRegisterLogoModel? Logo { get; set; }

        public DateTime GetFiscalYearStart()
        {
            return DateTime.Today.Month < FiscalYearStartMonth
                ? new DateTime(DateTime.Today.Year - 1, FiscalYearStartMonth, 1)
                : new DateTime(DateTime.Today.Year, FiscalYearStartMonth, 1);
        }
    }
}
