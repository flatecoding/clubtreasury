namespace ClubTreasury.Data.Mapper.DTOs;

public class BudgetFlatEntryDto
{
    public int CostCenterId { get; init; }
    public string CostCenterName { get; init; } = string.Empty;

    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;

    public int? ItemDetailId { get; init; }
    public string? ItemDetailName { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public int? PersonId { get; init; }
    public string? PersonName { get; init; }
}


    public class CostCenterDto
    {
        public int Id { get; set; }
        public string CostUnitName { get; set; } = "";
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class ItemDetailDto
    {
        public int Id { get; set; }
        public string CostDetails { get; set; } = "";
    }

    public class PersonDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }