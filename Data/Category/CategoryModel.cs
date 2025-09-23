using System.ComponentModel.DataAnnotations;
using TTCCashRegister.Data.Accounts;
using TTCCashRegister.Data.CostCenter;

namespace TTCCashRegister.Data.Category
{
    public class CategoryModel
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
