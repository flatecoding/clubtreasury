using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTCCashRegister.Data.CostUnit;
using TTCCashRegister.Data.UnitDetail;

namespace TTCCashRegister.Data.BasicUnit
{
    public class BasicUnitModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public ICollection<UnitDetailsModel> CostUnitDetails { get; set; } =  new List<UnitDetailsModel>();
        public int? CostUnitId { get; set; }
        [ForeignKey("CostUnitId")]
        public CostUnitModel? CostUnit { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
