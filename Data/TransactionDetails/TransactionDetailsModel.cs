using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClubTreasury.Data.Person;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Data.TransactionDetails;

public class TransactionDetailsModel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int Id { get; init; }

    [Required]
    public int TransactionId { get; set; }
    [ForeignKey("TransactionId")]
    public TransactionModel Transaction { get; init; } = null!;
    
    public int? DocumentNumber { get; set; }

    [MaxLength(300)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Sum { get; set; }
    
    public int? PersonId { get; set; }
    [ForeignKey("PersonId")]
    public PersonModel? Person { get; init; }
}