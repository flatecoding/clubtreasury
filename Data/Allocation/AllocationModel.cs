using System.ComponentModel.DataAnnotations;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;
using TTCCashRegister.Data.ItemDetail;
using TTCCashRegister.Data.Transaction;

namespace TTCCashRegister.Data.Allocation;

public class AllocationModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CostCenterId { get; set; }
    public CostCenterModel CostCenter { get; set; } = null!;

    [Required]
    public int CategoryId { get; set; }
    public CategoryModel Category { get; set; } = null!;

    public int? ItemDetailId { get; set; } // optional
    public ItemDetailModel? ItemDetail { get; set; }

    // Navigation
    public ICollection<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
}