using System.ComponentModel.DataAnnotations;
using ClubTreasury.Data.Allocation;

namespace ClubTreasury.Data.ItemDetail
{
    public class ItemDetailModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string CostDetails { get; set; } = string.Empty;
        public ICollection<AllocationModel> Allocations { get; set; } = new List<AllocationModel>();
    }
}
