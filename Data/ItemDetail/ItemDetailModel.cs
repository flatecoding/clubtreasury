using System.ComponentModel.DataAnnotations;
using TTCCashRegister.Data.Accounts;

namespace TTCCashRegister.Data.ItemDetail
{
    public class ItemDetailModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string CostDetails { get; set; } = string.Empty;
        public ICollection<AccountsModel> Accounts { get; set; } = new List<AccountsModel>();
    }
}
