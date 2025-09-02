using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTCCashRegister.Data.BasicUnit;
using TTCCashRegister.Data.CashRegister;
using TTCCashRegister.Data.CostUnit;
using TTCCashRegister.Data.Person;
using TTCCashRegister.Data.SpecialItem;
using TTCCashRegister.Data.SubTransaction;
using TTCCashRegister.Data.UnitDetail;

namespace TTCCashRegister.Data.Transaction
{
    public class TransactionModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; init; }
        [Required]
        public DateOnly? Date { get; set; } = new DateOnly();
        [Required]
        public int Documentnumber { get; set; }
        [MaxLength(300)]
        public string? Description { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Sum { get; set; } = decimal.Zero;
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal AccountMovement { get; set; } = decimal.Zero;
        [Required]
        public int CostUnitId { get; set; }
        [ForeignKey("CostUnitId")]
        public CostUnitModel CostUnit { get; set; } = new();
        [Required]
        public int BasicUnitId { get; set; }
        [ForeignKey("BasicUnitId")]
        public BasicUnitModel? BasicUnit { get; set; } = new();
        public int? UnitDetailsId { get; set; } // Optional
        [ForeignKey("UnitDetailsId")]
        public UnitDetailsModel? UnitDetails { get; set; } // Optional
        [Required]
        public int CashRegisterId { get; set; }
        [ForeignKey("CashRegisterId")]
        public CashRegisterModel? CashRegister { get; set; }
        public int? SpecialItemId { get; set; }

        [ForeignKey("SpecialItemId")]
        public SpecialItemModel? SpecialItem { get; set; }
        public int? PersonId { get; set; }
        public PersonModel? Person { get; set; }

        public List<SubTransactionModel>? SubTransactions { get; init; } = [];

    }
}
