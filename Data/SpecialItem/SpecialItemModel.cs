using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTCCashRegister.Data.SpecialItem
{
    public class SpecialItemModel
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
