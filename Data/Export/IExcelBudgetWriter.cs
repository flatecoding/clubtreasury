using TTCCashRegister.Data.Mapper.DTOs;

namespace TTCCashRegister.Data.Export;

public interface IExcelBudgetWriter
{
        Task WriteAsync(string filePath, IEnumerable<BudgetGroupedDto> grouped, DateTime begin, DateTime end);
}