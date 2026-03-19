using System.Drawing;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing.Chart.Style;
using OfficeOpenXml.Style;
using ClubTreasury.Data.Mapper.DTOs;

namespace ClubTreasury.Data.Export.Budget;

internal class BudgetChartSheetWriter(IStringLocalizer<Translation> localizer)
{
    private const int CostCenterChartWidth = 200;
    private const int CostCenterChartHeight = 400;
    private const int CategoryChartWidth = 300;
    private const int CategoryChartHeight = 450;

    private const int MinCategoryPercentageThreshold = 2;

    public void Write(ExcelPackage package, List<BudgetGroupedDto> groupedList)
    {
        var costCenterExpenses = groupedList
            .Where(cc => cc.SumCostCenter < 0)
            .Select(cc => (Name: cc.CostUnitName, Amount: Math.Abs(cc.SumCostCenter)))
            .ToList();

        var allCategoryExpenses = groupedList
            .SelectMany(cc => cc.Categories)
            .Where(cat => cat.SumCategories < 0)
            .GroupBy(cat => cat.CategoryName)
            .Select(g => (Name: g.Key, Amount: Math.Abs(g.Sum(c => c.SumCategories))))
            .ToList();

        var totalCategoryAmount = allCategoryExpenses.Sum(c => c.Amount);
        var categoryExpenses = totalCategoryAmount > 0
            ? allCategoryExpenses.Where(c => Math.Round(c.Amount / totalCategoryAmount * 100) >= MinCategoryPercentageThreshold).ToList()
            : allCategoryExpenses;
        var smallCategoryExpenses = totalCategoryAmount > 0
            ? allCategoryExpenses.Where(c => Math.Round(c.Amount / totalCategoryAmount * 100) < MinCategoryPercentageThreshold).ToList()
            : [];

        if (costCenterExpenses.Count == 0 && categoryExpenses.Count == 0)
            return;

        var ws = package.Workbook.Worksheets.Add(localizer["Charts"]);
        var row = 1;

        if (costCenterExpenses.Count > 0)
        {
            row = WriteExpenseDataAndChart(ws, localizer["ExpensesByCostCenter"],
                costCenterExpenses, row, CostCenterChartWidth, CostCenterChartHeight);
            row += 23;
        }

        if (categoryExpenses.Count > 0)
        {
            row++;
            if (smallCategoryExpenses.Count > 0)
                WriteSmallCategoriesTable(ws, smallCategoryExpenses, totalCategoryAmount, row + 1);

            WriteExpenseDataAndChart(ws, localizer["ExpensesByCategory"],
                categoryExpenses, row, CategoryChartWidth, CategoryChartHeight);
        }

        ws.Cells.AutoFitColumns();
    }

    private static int WriteExpenseDataAndChart(ExcelWorksheet ws, string title,
        List<(string Name, decimal Amount)> expenses, int row, int chartWidth, int chartHeight)
    {
        var totalAmount = expenses.Sum(e => e.Amount);

        ws.Cells[row, 1].Value = title;
        ws.Cells[row, 1].Style.Font.Bold = true;
        ws.Cells[row, 1].Style.Font.Size = 14;
        row++;

        var dataStartRow = row;
        foreach (var item in expenses)
        {
            ws.Cells[row, 1].Value = item.Name;
            ws.Cells[row, 2].Value = (double)item.Amount;
            ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00 €";
            ws.Cells[row, 3].Value = totalAmount > 0 ? (double)(item.Amount / totalAmount) : 0d;
            ws.Cells[row, 3].Style.Numberformat.Format = "0.00%";
            row++;
        }

        var dataEndRow = row - 1;
        row++;

        AddPieChart(ws, title, row - 1, dataStartRow, dataEndRow, chartWidth, chartHeight);

        return row;
    }

    private void WriteSmallCategoriesTable(ExcelWorksheet ws,
        List<(string Name, decimal Amount)> smallExpenses, decimal totalAmount, int startRow)
    {
        const int colOffset = 5;

        ws.Cells[startRow, colOffset].Value = localizer["Category"];
        ws.Cells[startRow, colOffset + 1].Value = localizer["Sum"];
        ws.Cells[startRow, colOffset + 2].Value = "%";
        using (var range = ws.Cells[startRow, colOffset, startRow, colOffset + 2])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.DarkGray);
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        var row = startRow + 1;
        foreach (var item in smallExpenses)
        {
            ws.Cells[row, colOffset].Value = item.Name;
            ws.Cells[row, colOffset + 1].Value = (double)item.Amount;
            ws.Cells[row, colOffset + 1].Style.Numberformat.Format = "#,##0.00 €";
            ws.Cells[row, colOffset + 2].Value = (double)(item.Amount / totalAmount);
            ws.Cells[row, colOffset + 2].Style.Numberformat.Format = "0.00%";
            row++;
        }
    }

    private static void AddPieChart(ExcelWorksheet ws, string title,
        int chartRow, int dataStartRow, int dataEndRow, int width, int height)
    {
        var chart = ws.Drawings.AddChart(title, eChartType.Pie);
        chart.SetPosition(chartRow, 0, 0, 0);
        chart.SetSize(width, height);
        chart.Title.Text = title;
        chart.StyleManager.SetChartStyle(ePresetChartStyle.PieChartStyle3);

        chart.Series.Add(
            ws.Cells[dataStartRow, 2, dataEndRow, 2],
            ws.Cells[dataStartRow, 1, dataEndRow, 1]);

        if (chart is ExcelPieChart pieChart)
        {
            pieChart.DataLabel.ShowCategory = false;
            pieChart.DataLabel.ShowPercent = true;
            pieChart.DataLabel.ShowValue = false;
        }
    }
}
