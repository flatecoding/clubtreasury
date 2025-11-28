namespace TTCCashRegister.Data.Export.DTOs;


public class BudgetGroupedDto
{
    public int CostCenterId { get; set; }
    public string CostUnitName { get; set; } = string.Empty;
    public decimal SumCostCenter { get; set; }
    public List<BudgetCategoryDto> Categories { get; set; } = new();
}

public class BudgetCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal SumCategories { get; set; }
    public List<BudgetItemDetailDto> ItemDetails { get; set; } = new();
}

public class BudgetItemDetailDto
{
    public int ItemDetailId { get; set; }
    public string ItemDetailName { get; set; } = string.Empty;
    public decimal SumItemDetails { get; set; }
    public List<BudgetPersonDto> Persons { get; set; } = new();
}

public class BudgetPersonDto
{
    public int PersonId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public decimal SumPerson { get; set; }
}

    