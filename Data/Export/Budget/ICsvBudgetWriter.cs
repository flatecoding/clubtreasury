using TTCCashRegister.Data.Mapper.DTOs;

namespace TTCCashRegister.Data.Export.Budget;

public interface ICsvBudgetWriter
{
        Task WriteAsync(string filePath, IEnumerable<BudgetGroupedDto> grouped);
}