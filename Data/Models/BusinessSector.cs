using System.ComponentModel.DataAnnotations;

namespace TTCCashRegister.Data.Models
{
    public class BusinessSector
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Sector { get; set; } = string.Empty;
    }
}
