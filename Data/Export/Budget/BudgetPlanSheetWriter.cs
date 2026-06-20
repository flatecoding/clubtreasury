using System.Drawing;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ClubTreasury.Data.Mapper.DTOs;

namespace ClubTreasury.Data.Export.Budget;

internal class BudgetPlanSheetWriter(IStringLocalizer<Translation> localizer)
{
    private const int NumberColumn = 1;
    private const int NameColumn = 2;
    private const int IncomeColumn = 3;
    private const int ExpensesColumn = 4;

    private const float TitleFontSize = 18;
    private const float SubtitleFontSize = 12;
    private const float CostCenterFontSize = 12;
    private const float CategoryFontSize = 11;

    private const string CostCenterRowColor = "#D9D9D9";
    private const string CategoryRowColor = "#EDEDED";

    public void Write(ExcelPackage package, List<BudgetPlanCostCenterDto> costCenters, int planningYear)
    {
        var ws = package.Workbook.Worksheets.Add(localizer["BudgetPlan"]);

        WriteTitle(ws, planningYear);
        WriteHeader(ws);

        var row = 5;
        var number = 1;

        foreach (var cc in costCenters)
        {
            WriteCostCenterRow(ws, ref row, number++, cc);

            foreach (var cat in cc.Categories)
            {
                WriteCategoryRow(ws, ref row, cat);
            }

            row++;
        }

        WriteSummary(ws, ref row, costCenters);

        ws.Cells[4, NumberColumn, row, ExpensesColumn].AutoFitColumns();
        ws.Calculate();
    }

    private void WriteSummary(ExcelWorksheet ws, ref int row, List<BudgetPlanCostCenterDto> costCenters)
    {
        var totalIncome = costCenters.Sum(cc => cc.Income);
        var totalExpenses = costCenters.Sum(cc => cc.Expenses);

        var totalRow = row;
        ws.Cells[totalRow, NameColumn].Value = localizer["TotalResult"];
        WriteSummaryAmount(ws, totalRow, IncomeColumn, (double)totalIncome);
        WriteSummaryAmount(ws, totalRow, ExpensesColumn, (double)totalExpenses);
        ws.Cells[totalRow, NumberColumn, totalRow, ExpensesColumn].Style.Border.Top.Style = ExcelBorderStyle.Thin;

        var incomeAddress = ws.Cells[totalRow, IncomeColumn].Address;
        var expensesAddress = ws.Cells[totalRow, ExpensesColumn].Address;

        var budgetRow = totalRow + 1;
        ws.Cells[budgetRow, NameColumn].Value = localizer["BudgetResult"];
        ws.Cells[budgetRow, IncomeColumn].Formula = $"{incomeAddress}-{expensesAddress}";
        ws.Cells[budgetRow, IncomeColumn].Style.Numberformat.Format = BudgetExportFormats.CurrencyFormat;

        var carryoverRow = budgetRow + 1;
        ws.Cells[carryoverRow, NameColumn].Value = localizer["PreviousYearCarryover"];
        ws.Cells[carryoverRow, IncomeColumn].Style.Numberformat.Format = BudgetExportFormats.CurrencyFormat;

        var finalRow = carryoverRow + 2;
        ws.Cells[finalRow, NameColumn].Value = localizer["FinalResult"];
        ws.Cells[finalRow, IncomeColumn].Formula =
            $"{ws.Cells[budgetRow, IncomeColumn].Address}+{ws.Cells[carryoverRow, IncomeColumn].Address}";
        ws.Cells[finalRow, IncomeColumn].Style.Numberformat.Format = BudgetExportFormats.CurrencyFormat;

        ws.Cells[totalRow, NameColumn, finalRow, NameColumn].Style.Font.Bold = true;
        ws.Cells[totalRow, IncomeColumn, finalRow, ExpensesColumn].Style.Font.Bold = true;

        row = finalRow;
    }

    private static void WriteSummaryAmount(ExcelWorksheet ws, int row, int col, double amount)
    {
        ws.Cells[row, col].Value = amount;
        ws.Cells[row, col].Style.Numberformat.Format = BudgetExportFormats.CurrencyFormat;
    }

    private void WriteTitle(ExcelWorksheet ws, int planningYear)
    {
        ws.Cells[1, NumberColumn].Value = localizer["BudgetPlan"];
        ws.Cells[1, NumberColumn].Style.Font.Bold = true;
        ws.Cells[1, NumberColumn].Style.Font.Size = TitleFontSize;

        ws.Cells[2, NumberColumn].Value = $"{localizer["PlanningYear"]}: {planningYear}";
        ws.Cells[2, NumberColumn].Style.Font.Italic = true;
        ws.Cells[2, NumberColumn].Style.Font.Size = SubtitleFontSize;
    }

    private void WriteHeader(ExcelWorksheet ws)
    {
        const int row = 4;
        ws.Cells[row, NumberColumn].Value = localizer["Number"];
        ws.Cells[row, NameColumn].Value = $"{localizer["CostCenter"]} / {localizer["Category"]}";
        ws.Cells[row, IncomeColumn].Value = localizer["Income"];
        ws.Cells[row, ExpensesColumn].Value = localizer["Expenses"];

        using var range = ws.Cells[row, NumberColumn, row, ExpensesColumn];
        range.Style.Font.Bold = true;
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(Color.DarkGray);
        range.Style.Font.Color.SetColor(Color.White);
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    }

    private static void WriteCostCenterRow(ExcelWorksheet ws, ref int row, int number, BudgetPlanCostCenterDto cc)
    {
        ws.Cells[row, NumberColumn].Value = number;
        ws.Cells[row, NumberColumn].Style.Font.Bold = true;

        ws.Cells[row, NameColumn].Value = cc.CostCenterName;
        ws.Cells[row, NameColumn].Style.Font.Bold = true;
        ws.Cells[row, NameColumn].Style.Font.Size = CostCenterFontSize;

        WriteAmount(ws, row, IncomeColumn, cc.Income);
        WriteAmount(ws, row, ExpensesColumn, cc.Expenses);

        using (var range = ws.Cells[row, NumberColumn, row, ExpensesColumn])
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml(CostCenterRowColor));
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        row += 2;
    }

    private static void WriteCategoryRow(ExcelWorksheet ws, ref int row, BudgetPlanCategoryDto cat)
    {
        ws.Cells[row, NameColumn].Value = cat.CategoryName;
        ws.Cells[row, NameColumn].Style.Font.Size = CategoryFontSize;
        ws.Cells[row, NameColumn].Style.Indent = 1;

        WriteAmount(ws, row, IncomeColumn, cat.Income);
        WriteAmount(ws, row, ExpensesColumn, cat.Expenses);

        using (var range = ws.Cells[row, NumberColumn, row, ExpensesColumn])
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml(CategoryRowColor));
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
        }

        row++;
    }

    private static void WriteAmount(ExcelWorksheet ws, int row, int col, decimal amount)
    {
        if (amount == 0)
            return;

        ws.Cells[row, col].Value = (double)amount;
        ws.Cells[row, col].Style.Numberformat.Format = BudgetExportFormats.CurrencyFormat;
    }
}
