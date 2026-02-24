using ClubTreasury.Data.Mapper.DTOs;

namespace ClubTreasury.Data.Export.Budget;

public interface IExcelBudgetWriter
{
        Task WriteAsync(string filePath, IEnumerable<BudgetGroupedDto> grouped, DateTime begin, DateTime end);
}