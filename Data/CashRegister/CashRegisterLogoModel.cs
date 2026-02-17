using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTCCashRegister.Data.CashRegister;

public class CashRegisterLogoModel
{
    [Key]
    public int Id { get; init; }
    public int CashRegisterId { get; set; }
    [ForeignKey("CashRegisterId")]
    public CashRegisterModel CashRegister { get; set; } = null!;
    public byte[] Data { get; set; } = [];
    [MaxLength(50)]
    public string ContentType { get; set; } = string.Empty;
}
