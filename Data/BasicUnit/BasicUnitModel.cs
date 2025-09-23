using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTCCashRegister.Data.Accounts;
using TTCCashRegister.Data.CostCenter;
using TTCCashRegister.Data.UnitDetail;

namespace TTCCashRegister.Data.BasicUnit
{
    public class BasicUnitModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int CostCenterId { get; set; }
        public CostCenterModel CostCenter { get; set; } = null!;

        public ICollection<AccountsModel> Accounts { get; set; } = new List<AccountsModel>();

        public override string ToString()
        {
            return Name;
        }
    }
}
