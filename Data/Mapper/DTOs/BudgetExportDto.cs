namespace TTCCashRegister.Data.Mapper.DTOs;

public class BudgetFlatEntryDto
{
    public int CostCenterId { get; set; }
    public string CostCenterName { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public int? ItemDetailId { get; set; }
    public string ItemDetailName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public int? PersonId { get; set; }
    public string? PersonName { get; set; }
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