using System.ComponentModel.DataAnnotations;
using TTCCashRegister.Data.BasicUnit;

namespace TTCCashRegister.Data.UnitDetail
{
    public class UnitDetailsModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CostDetails { get; set; } = string.Empty;
        public IEnumerable<BasicUnitModel> BasicUnits { get; set; } = new List<BasicUnitModel>();
    }
}
