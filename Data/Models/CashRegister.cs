using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTCCashRegister.Data.Models
{
    public class CashRegister
    {
        [Key]
        public int ID { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal CurrentBalance { get; set; } = decimal.Zero;
        public ICollection<Entry> Entries { get; set; }

        public CashRegister()
        {
            Entries = new List<Entry>();

        }
    }
}
