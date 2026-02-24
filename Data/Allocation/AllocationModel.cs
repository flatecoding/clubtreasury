using System.ComponentModel.DataAnnotations;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.ItemDetail;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Data.Allocation;

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