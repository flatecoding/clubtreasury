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
        public int CostUnitId { get; set; }
        [ForeignKey("CostUnitId")]
        public CostUnit? CostUnit { get; set; }
    }
}
