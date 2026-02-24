using System.ComponentModel.DataAnnotations;

namespace ClubTreasury.Data.Person;

public class PersonModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}