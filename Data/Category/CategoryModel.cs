using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.CostCenter;

namespace TTCCashRegister.Data.Category
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<AllocationModel> Allocations { get; set; } = new List<AllocationModel>();

        public override string ToString()
        {
            return Name;
        }

        // Optional: Zugriff auf verknüpfte Kostenstellen
        [NotMapped]
        public IEnumerable<CostCenterModel> LinkedCostCenters => Allocations.Select(a => a.CostCenter).Distinct();
    }
}