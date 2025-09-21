using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTCCashRegister.Data.Accounts;
using TTCCashRegister.Data.BasicUnit;


namespace TTCCashRegister.Data.CostUnit
{
    public class CostUnitModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The cost unit name cannot exceed 100 characters.")]
        public string CostUnitName { get; set; } = string.Empty;
        public ICollection<BasicUnitModel> BasicUnitDetails { get; set; } = new List<BasicUnitModel>();
        public ICollection<AccountsModel> Accounts { get; set; } = new List<AccountsModel>();

        [NotMapped]
        public int BasicUnitDetailsCount => BasicUnitDetails.Count;
    }
}
