using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.Category;

namespace TTCCashRegister.Data.CostCenter
{
    public class CostCenterModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The cost center name cannot exceed 100 characters.")]
        public string CostUnitName { get; set; } = string.Empty;
        public ICollection<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        public ICollection<AllocationModel> Accounts { get; set; } = new List<AllocationModel>();

        [NotMapped]
        public int CategoriesCount => Categories.Count;
    }
}
