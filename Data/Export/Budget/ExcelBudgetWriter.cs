using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using ClubTreasury.Data.Mapper.DTOs;

namespace ClubTreasury.Data.Export.Budget;

public class ExcelBudgetWriter(IStringLocalizer<Translation> localizer) : IExcelBudgetWriter
{
    private readonly BudgetSheetWriter _budgetSheetWriter = new(localizer);
    private readonly BudgetChartSheetWriter _chartSheetWriter = new(localizer);

    public async Task WriteAsync(string filePath, IEnumerable<BudgetGroupedDto> grouped, DateTime begin, DateTime end)
    {
        using var package = new ExcelPackage();
        var groupedList = grouped.ToList();

        _budgetSheetWriter.Write(package, groupedList, begin, end);
        _chartSheetWriter.Write(package, groupedList);

        await package.SaveAsAsync(new FileInfo(filePath));
    }
}
