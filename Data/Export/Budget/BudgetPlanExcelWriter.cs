using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using ClubTreasury.Data.Mapper.DTOs;

namespace ClubTreasury.Data.Export.Budget;

public class BudgetPlanExcelWriter(IStringLocalizer<Translation> localizer) : IBudgetPlanExcelWriter
{
    private readonly BudgetPlanSheetWriter _budgetPlanSheetWriter = new(localizer);

    public async Task WriteAsync(string filePath, IEnumerable<BudgetPlanCostCenterDto> costCenters, int planningYear)
    {
        using var package = new ExcelPackage();

        _budgetPlanSheetWriter.Write(package, costCenters.ToList(), planningYear);

        await package.SaveAsAsync(new FileInfo(filePath));
    }
}
