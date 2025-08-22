using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTCCashRegister.Data.Transaction;

namespace TTCCashRegister.Data.CashRegister
{
    public class CashRegisterModel
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        [NotMapped] 
        public decimal CurrentBalance => Transactions?.Sum(t => t.AccountMovement) ?? 0m;
        public ICollection<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
    }
}
