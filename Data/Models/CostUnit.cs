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
        public ICollection<BasicUnit> BasicUnitDetails { get; set; } = new List<BasicUnit>();

        [NotMapped]
        public int BasicUnitDetailsCount => BasicUnitDetails?.Count ?? 0;
    }
}
