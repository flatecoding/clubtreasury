using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TTCCashRegister.Data.Models
{
    public class SpecialItem
    {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Key]
            public int Id { get; set; }

            [Required]
            public string Name { get; set; } = string.Empty;

            [Column(TypeName = "decimal(10,2)")]
            public decimal Betrag { get; set; } = decimal.Zero;
    }
}
