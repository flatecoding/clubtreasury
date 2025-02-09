using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTCCashRegister.Data.BasicUnit;

namespace TTCCashRegister.Data.UnitDetail
{
    public class UnitDetailsModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CostDetails { get; set; } = string.Empty;
        public int BasicUnitId { get; set; }
        [ForeignKey("BasicUnitId")]
        public BasicUnitModel? BasicUnit { get; set; }
    }
}
