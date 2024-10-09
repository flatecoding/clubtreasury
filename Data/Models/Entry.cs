using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTCCashRegister.Data.Models
{
    public class Entry
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateOnly? Date { get; set; } = new DateOnly();
        [Required]
        public int Documentnumber { get; set; }
        public string Description { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Transaction { get; set; } = decimal.Zero;
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal BankTransaction { get; set; } = decimal.Zero;
        [Required]
        public int SectorId { get; set; }
        [ForeignKey("SectorId")]
        public virtual BusinessSector? BusinessSector { get; set; }
    }
}
