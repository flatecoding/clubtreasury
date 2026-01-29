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

        public ICollection<AllocationModel> Allocations { get; set; } = new List<AllocationModel>();

        [NotMapped]
        public int AllocationsCount => Allocations.Count;

        // Optional: Zugriff auf verknüpfte Kategorien
        [NotMapped]
        public IEnumerable<CategoryModel> LinkedCategories => Allocations
            .Select(a => a.Category).Distinct();
        
        public override bool Equals(object? obj) {
            var other = obj as CostCenterModel;
            return other?.CostUnitName == CostUnitName;
        }
        
        public override int GetHashCode() => CostUnitName.GetHashCode();
    }
}