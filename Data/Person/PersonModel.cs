using System.ComponentModel.DataAnnotations;

namespace TTCCashRegister.Data.Person;

public class PersonModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}