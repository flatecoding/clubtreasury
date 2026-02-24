using System.Text;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Mapper.DTOs;

namespace ClubTreasury.Data.Export.Budget;

public class CsvBudgetWriter(IStringLocalizer<Translation> localizer) : ICsvBudgetWriter
{
    public async Task WriteAsync(string filePath, IEnumerable<BudgetGroupedDto> grouped)
    {
        var sb = new StringBuilder();

        var csvHeader = string.Join(";",new[]
        {
            localizer["CostCenter"].Value,
            localizer["Category"].Value,
            localizer["DetailOrPerson"].Value,
            localizer["Sum"].Value,
        });
        sb.AppendLine(csvHeader);

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