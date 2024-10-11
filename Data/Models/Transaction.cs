using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTCCashRegister.Data.Models
{
    public class Transaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        [Required]
        public DateOnly? Date { get; set; } = new DateOnly();
        [Required]
        public int Documentnumber { get; set; }
        public string Description { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Sum { get; set; } = decimal.Zero;
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal AccountMovement { get; set; } = decimal.Zero;
        [Required]
        public int CostUnitID { get; set; }
        [ForeignKey("CostUnitID")]
        public CostUnit CostUnit { get; set; } = new();
        public int CashRegisterID { get; set; }
        [ForeignKey("CashRegisterID")]
        public CashRegister? CashRegister { get; set; }
    }
}
