using System.ComponentModel.DataAnnotations;
using TTCCashRegister.Data.Allocation;

namespace TTCCashRegister.Data.ItemDetail
{
    public class ItemDetailModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string CostDetails { get; set; } = string.Empty;
        public ICollection<AllocationModel> Accounts { get; set; } = new List<AllocationModel>();
    }
}
