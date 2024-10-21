using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTCCashRegister.Data.Models
{
    public class UnitDetails
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CostDetails { get; set; } = string.Empty;
        public int BasicUnitId { get; set; }
        [ForeignKey("BasicUnitId")]
        public BasicUnit? BasicUnit { get; set; }
    }
}
