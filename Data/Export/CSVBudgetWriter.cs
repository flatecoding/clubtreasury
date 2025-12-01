using System.Text;
using TTCCashRegister.Data.Mapper.DTOs;

namespace TTCCashRegister.Data.Export;

public class CsvBudgetWriter : ICsvBudgetWriter
{
    private const string CsvBudgetHeader = "Kostenstelle;Details/Person;Details;Summe";

    public async Task WriteAsync(string filePath, IEnumerable<BudgetGroupedDto> grouped)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CsvBudgetHeader);

        foreach (var line in BudgetLineBuilder.EnumerateBudgetLines(grouped))
        {
            var col1 = line.CostCenter      ?? string.Empty;
            var col2 = line.Category        ?? string.Empty;
            var col3 = line.DetailOrPerson  ?? string.Empty;
            var col4 = line.Amount.ToString("C"); // oder eigenes Format

            sb.AppendLine($"{col1};{col2};{col3};{col4}");
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }
    
}