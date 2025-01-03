using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTCCashRegister.Data.Transaction;

namespace TTCCashRegister.Data.CashRegister
{
    public class CashRegisterModel
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "decimal(10,2)")]
        public decimal CurrentBalance { get; set; } = decimal.Zero;
        public ICollection<TransactionModel> Transactions { get; set; }

        public CashRegisterModel()
        {
            Transactions = new List<TransactionModel>();

        }
    }
}
