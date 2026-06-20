namespace ClubTreasury.Data.Mapper.DTOs;

public class BudgetPlanCostCenterDto
{
    public int CostCenterId { get; set; }
    public string CostCenterName { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public List<BudgetPlanCategoryDto> Categories { get; set; } = new();
}

public class BudgetPlanCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
}
