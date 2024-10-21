using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTCCashRegister.Data.Models
{
    public class BasicUnit
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public ICollection<UnitDetails> CostUnitDetails { get; set; } = new List<UnitDetails>();
        public int? CostUnitId { get; set; }
        [ForeignKey("CostUnitId")]
        public CostUnit? CostUnit { get; set; }
    }
}
