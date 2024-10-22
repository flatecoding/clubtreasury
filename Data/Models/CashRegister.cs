using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTCCashRegister.Data.Models
{
    public class CashRegister
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "decimal(10,2)")]
        public decimal CurrentBalance { get; set; } = decimal.Zero;
        public ICollection<Transaction> Transactions { get; set; }

        public CashRegister()
        {
            Transactions = new List<Transaction>();

        }
    }
}
