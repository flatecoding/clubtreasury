using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTCCashRegister.Data.Models
{
    public class CostUnit
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The cost unit name cannot exceed 100 characters.")]
        public string CostUnitName { get; set; } = string.Empty;
        public ICollection<UnitDetails>? CostUnitDetails { get; set; }

        [NotMapped]
        public int CostUnitDetailsCount => CostUnitDetails?.Count ?? 0;
    }
}
