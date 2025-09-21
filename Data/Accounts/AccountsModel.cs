using System.ComponentModel.DataAnnotations;
using TTCCashRegister.Data.BasicUnit;
using TTCCashRegister.Data.CostUnit;
using TTCCashRegister.Data.Transaction;
using TTCCashRegister.Data.UnitDetail;

namespace TTCCashRegister.Data.Accounts;

public class AccountsModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CostUnitId { get; set; }
    public CostUnitModel CostUnit { get; set; } = null!;

    [Required]
    public int BasicUnitId { get; set; }
    public BasicUnitModel BasicUnit { get; set; } = null!;

    public int? UnitDetailsId { get; set; } // optional
    public UnitDetailsModel? UnitDetails { get; set; }

    // Navigation
    public ICollection<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
}