using ClubTreasury.Data.Mapper.DTOs;

namespace ClubTreasury.Data.Export.Budget;

public interface IBudgetPlanExcelWriter
{
    Task WriteAsync(string filePath, IEnumerable<BudgetPlanCostCenterDto> costCenters, int planningYear);
}
