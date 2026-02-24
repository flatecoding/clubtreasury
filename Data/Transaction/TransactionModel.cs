using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.SpecialItem;
using ClubTreasury.Data.TransactionDetails;

namespace ClubTreasury.Data.Transaction
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
        public int AllocationId { get; set; }
        public AllocationModel Allocation { get; set; } = null!;
        
        [Required]
        public int CashRegisterId { get; set; }
        [ForeignKey("CashRegisterId")]
        public CashRegisterModel? CashRegister { get; set; }
        public int? SpecialItemId { get; set; }

        [ForeignKey("SpecialItemId")]
        public SpecialItemModel? SpecialItem { get; set; }
        public List<TransactionDetailsModel> TransactionDetails { get; } = [];

    }
}
