using System.ComponentModel.DataAnnotations;
using TTCCashRegister.Data.Accounts;
using TTCCashRegister.Data.BasicUnit;

namespace TTCCashRegister.Data.UnitDetail
{
    public class UnitDetailsModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CostDetails { get; set; } = string.Empty;
        public ICollection<AccountsModel> Accounts { get; set; } = new List<AccountsModel>();
    }
}
