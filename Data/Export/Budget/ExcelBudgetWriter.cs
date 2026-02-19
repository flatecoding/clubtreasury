using System.Drawing;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing.Chart.Style;
using OfficeOpenXml.Style;
using TTCCashRegister.Data.Mapper.DTOs;

namespace TTCCashRegister.Data.Export.Budget;

public class ExcelBudgetWriter(IStringLocalizer<Translation> localizer) : IExcelBudgetWriter
{
    public async Task WriteAsync(string filePath, IEnumerable<BudgetGroupedDto> grouped, DateTime begin, DateTime end)
    {
        using var package = new ExcelPackage();
        var groupedList = grouped.ToList();

        WriteBudgetSheet(package, groupedList, begin, end);
        WriteChartsSheet(package, groupedList);

        await package.SaveAsAsync(new FileInfo(filePath));
    }

    private void WriteBudgetSheet(ExcelPackage package, List<BudgetGroupedDto> groupedList, DateTime begin, DateTime end)
    {
        var ws = package.Workbook.Worksheets.Add(localizer["Budget"]);

        // Titel
        ws.Cells[1, 1].Value = localizer["BalanceSheetTitle"];
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.Font.Size = 18;

        ws.Cells[2, 1].Value = $"{localizer["DateRange"]}: {begin:dd.MM.yyyy} - {end:dd.MM.yyyy}";
        ws.Cells[2, 1].Style.Font.Italic = true;
        ws.Cells[2, 1].Style.Font.Size = 12;

        // Header
        int row = 4;
        ws.Cells[row, 1].Value = localizer["CostCenter"];
        ws.Cells[row, 2].Value = localizer["Category"];
        ws.Cells[row, 3].Value = localizer["DetailOrPerson"];
        ws.Cells[row, 4].Value = localizer["Sum"];

        using (var range = ws.Cells[row, 1, row, 4])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.DarkGray);
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        row++;

        int detailRowCounter = 0;

        foreach (var cc in groupedList)
        {
            ws.Cells[row, 1].Value = cc.CostUnitName;
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 12;

            ws.Cells[row, 4].Value = (double)cc.SumCostCenter;
            ws.Cells[row, 4].Style.Numberformat.Format = "#,##0.00 €";

            using (var range = ws.Cells[row, 1, row, 4])
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#D9D9D9"));
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            row += 2;

            foreach (var cat in cc.Categories)
            {
                ws.Cells[row, 2].Value = cat.CategoryName;
                ws.Cells[row, 2].Style.Font.Bold = true;
                ws.Cells[row, 2].Style.Font.Size = 11;

                ws.Cells[row, 4].Value = (double)cat.SumCategories;
                ws.Cells[row, 4].Style.Numberformat.Format = "#,##0.00 €";

                using (var range = ws.Cells[row, 2, row, 4])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#EDEDED"));
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                }

                row++;

                foreach (var item in cat.ItemDetails)
                {
                    decimal personSum = item.Persons.Sum(p => p.SumPerson);

                    bool showItemRow =
                        !string.IsNullOrWhiteSpace(item.ItemDetailName) ||
                        item.Persons.Count == 0 ||
                        personSum != item.SumItemDetails;

                    if (showItemRow)
                    {
                        detailRowCounter++;

                        bool altBackground = detailRowCounter % 2 == 0;

                        ws.Cells[row, 3].Value = item.ItemDetailName;

                        using (var range = ws.Cells[row, 3, row, 4])
                        {
                            if (altBackground)
                            {
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#F6F6F6"));
                            }
                        }

                        ws.Cells[row, 4].Value = (double)item.SumItemDetails;
                        ws.Cells[row, 4].Style.Numberformat.Format = "#,##0.00 €";

                        row++;
                    }

                    foreach (var p in item.Persons)
                    {
                        ws.Cells[row, 3].Value = $"      {p.PersonName}";
                        ws.Cells[row, 3].Style.Font.Italic = true;

                        ws.Cells[row, 4].Value = (double)p.SumPerson;
                        ws.Cells[row, 4].Style.Numberformat.Format = "#,##0.00 €";

                        ws.Cells[row, 3].Style.Indent = 2;

                        row++;
                    }
                }
                row++;
            }
        }
        ws.Cells.AutoFitColumns();
    }

    private void WriteChartsSheet(ExcelPackage package, List<BudgetGroupedDto> groupedList)
    {
        var costCenterExpenses = groupedList
            .Where(cc => cc.SumCostCenter < 0)
            .Select(cc => new { cc.CostUnitName, Amount = Math.Abs(cc.SumCostCenter) })
            .ToList();
        
        var allCategoryExpenses = groupedList
            .SelectMany(cc => cc.Categories)
            .Where(cat => cat.SumCategories < 0)
            .GroupBy(cat => cat.CategoryName)
            .Select(g => new { CategoryName = g.Key, Amount = Math.Abs(g.Sum(c => c.SumCategories)) })
            .ToList();

        var totalCategoryAmount = allCategoryExpenses.Sum(c => c.Amount);
        var categoryExpenses = totalCategoryAmount > 0
            ? allCategoryExpenses.Where(c => Math.Round(c.Amount / totalCategoryAmount * 100) >= 2).ToList()
            : allCategoryExpenses;
        var smallCategoryExpenses = totalCategoryAmount > 0
            ? allCategoryExpenses.Where(c => Math.Round(c.Amount / totalCategoryAmount * 100) < 2).ToList()
            : [];

        if (costCenterExpenses.Count == 0 && categoryExpenses.Count == 0)
            return;

        var ws = package.Workbook.Worksheets.Add(localizer["Charts"]);
        var totalCostCenterAmount = costCenterExpenses.Sum(c => c.Amount);
        var row = 1;
        
        if (costCenterExpenses.Count > 0)
        {
            ws.Cells[row, 1].Value = localizer["ExpensesByCostCenter"];
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 14;
            row++;

            var dataStartRow = row;
            foreach (var item in costCenterExpenses)
            {
                ws.Cells[row, 1].Value = item.CostUnitName;
                ws.Cells[row, 2].Value = (double)item.Amount;
                ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00 €";
                ws.Cells[row, 3].Value = totalCostCenterAmount > 0 ? (double)(item.Amount / totalCostCenterAmount) : 0d;
                ws.Cells[row, 3].Style.Numberformat.Format = "0.00%";
                row++;
            }
            var dataEndRow = row - 1;

            row++;
            var chartRow = row;

            var costCenterChart = ws.Drawings.AddChart(localizer["ExpensesByCostCenter"], eChartType.Pie);
            costCenterChart.SetPosition(chartRow - 1, 0, 0, 0);
            costCenterChart.SetSize(200, 400);
            costCenterChart.Title.Text = localizer["ExpensesByCostCenter"];
            costCenterChart.StyleManager.SetChartStyle(ePresetChartStyle.PieChartStyle3);

            costCenterChart.Series.Add(
                ws.Cells[dataStartRow, 2, dataEndRow, 2],
                ws.Cells[dataStartRow, 1, dataEndRow, 1]);

            if (costCenterChart is ExcelPieChart chart)
            {
                chart.DataLabel.ShowCategory = false;
                chart.DataLabel.ShowPercent = true;
                chart.DataLabel.ShowValue = false;
            }

            row = chartRow + 23;
        }
        
        if (categoryExpenses.Count > 0)
        {
            row++;

            ws.Cells[row, 1].Value = localizer["ExpensesByCategory"];
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 14;
            row++;

            var dataStartRow = row;
            
            if (smallCategoryExpenses.Count > 0)
            {
                var smallTableRow = dataStartRow - 1; 
                var colOffset = 5; 

                ws.Cells[smallTableRow, colOffset].Value = localizer["Category"];
                ws.Cells[smallTableRow, colOffset + 1].Value = localizer["Sum"];
                ws.Cells[smallTableRow, colOffset + 2].Value = "%";
                using (var range = ws.Cells[smallTableRow, colOffset, smallTableRow, colOffset + 2])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkGray);
                    range.Style.Font.Color.SetColor(Color.White);
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                smallTableRow++;

                foreach (var item in smallCategoryExpenses)
                {
                    ws.Cells[smallTableRow, colOffset].Value = item.CategoryName;
                    ws.Cells[smallTableRow, colOffset + 1].Value = (double)item.Amount;
                    ws.Cells[smallTableRow, colOffset + 1].Style.Numberformat.Format = "#,##0.00 €";
                    ws.Cells[smallTableRow, colOffset + 2].Value = (double)(item.Amount / totalCategoryAmount);
                    ws.Cells[smallTableRow, colOffset + 2].Style.Numberformat.Format = "0.00%";
                    smallTableRow++;
                }
            }

            foreach (var item in categoryExpenses)
            {
                ws.Cells[row, 1].Value = item.CategoryName;
                ws.Cells[row, 2].Value = (double)item.Amount;
                ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00 €";
                ws.Cells[row, 3].Value = totalCategoryAmount > 0 ? (double)(item.Amount / totalCategoryAmount) : 0d;
                ws.Cells[row, 3].Style.Numberformat.Format = "0.00%";
                row++;
            }
            var dataEndRow = row - 1;

            row++;
            var chartRow = row;

            var categoryChart = ws.Drawings.AddChart(localizer["ExpensesByCategory"], eChartType.Pie);
            categoryChart.SetPosition(chartRow - 1, 0, 0, 0);
            categoryChart.SetSize(300, 450);
            categoryChart.Title.Text = localizer["ExpensesByCategory"];
            categoryChart.StyleManager.SetChartStyle(ePresetChartStyle.PieChartStyle3);

            categoryChart.Series.Add(
                ws.Cells[dataStartRow, 2, dataEndRow, 2],
                ws.Cells[dataStartRow, 1, dataEndRow, 1]);

            if (categoryChart is ExcelPieChart chart)
            {
                chart.DataLabel.ShowCategory = false;
                chart.DataLabel.ShowPercent = true;
                chart.DataLabel.ShowValue = false;
            }
        }

        ws.Cells.AutoFitColumns();
    }
}
