using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Data.SpecialItem
{
    public class SpecialItemModel
    {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Key]
            public int Id { get; init; }
            [Required]
            [MaxLength(50)]
            public string Name { get; set; } = string.Empty;
            public ICollection<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
            [NotMapped]
            public decimal Sum => Transactions?.Sum(t => t.AccountMovement) ?? 0m;
    }
}
