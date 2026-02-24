using ClubTreasury.Data.Mapper.DTOs;

namespace ClubTreasury.Data.Export.Budget;

public interface ICsvBudgetWriter
{
        Task WriteAsync(string filePath, IEnumerable<BudgetGroupedDto> grouped);
}