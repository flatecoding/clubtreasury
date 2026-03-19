using System.Drawing;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ClubTreasury.Data.Mapper.DTOs;

namespace ClubTreasury.Data.Export.Budget;

internal class BudgetSheetWriter(IStringLocalizer<Translation> localizer)
{
    private const float TitleFontSize = 18;
    private const float SubtitleFontSize = 12;
    private const float CostCenterFontSize = 12;
    private const float CategoryFontSize = 11;

    private const string CostCenterRowColor = "#D9D9D9";
    private const string CategoryRowColor = "#EDEDED";
    private const string AlternatingRowColor = "#F6F6F6";

    public void Write(ExcelPackage package, List<BudgetGroupedDto> groupedList, DateTime begin, DateTime end)
    {
        var ws = package.Workbook.Worksheets.Add(localizer["Budget"]);

        WriteTitle(ws, begin, end);
        WriteHeader(ws);

        var row = 5;
        var detailRowCounter = 0;

        foreach (var cc in groupedList)
        {
            WriteCostCenterRow(ws, ref row, cc);

            foreach (var cat in cc.Categories)
            {
                WriteCategoryRow(ws, ref row, cat);

                foreach (var item in cat.ItemDetails)
                {
                    WriteItemDetailRows(ws, ref row, ref detailRowCounter, item);
                }

                row++;
            }
        }

        ws.Cells[4, 1, row, 3].AutoFitColumns();
    }

    private void WriteTitle(ExcelWorksheet ws, DateTime begin, DateTime end)
    {
        ws.Cells[1, 1].Value = localizer["BalanceSheetTitle"];
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.Font.Size = TitleFontSize;

        ws.Cells[2, 1].Value = $"{localizer["DateRange"]}: {begin:dd.MM.yyyy} - {end:dd.MM.yyyy}";
        ws.Cells[2, 1].Style.Font.Italic = true;
        ws.Cells[2, 1].Style.Font.Size = SubtitleFontSize;
    }

    private void WriteHeader(ExcelWorksheet ws)
    {
        const int row = 4;
        ws.Cells[row, 1].Value = $"{localizer["CostCenter"]} / {localizer["Category"]}";
        ws.Cells[row, 2].Value = localizer["DetailOrPerson"];
        ws.Cells[row, 3].Value = localizer["Sum"];

        using var range = ws.Cells[row, 1, row, 3];
        range.Style.Font.Bold = true;
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(Color.DarkGray);
        range.Style.Font.Color.SetColor(Color.White);
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    }

    private static void WriteCostCenterRow(ExcelWorksheet ws, ref int row, BudgetGroupedDto cc)
    {
        ws.Cells[row, 1].Value = cc.CostUnitName;
        ws.Cells[row, 1].Style.Font.Bold = true;
        ws.Cells[row, 1].Style.Font.Size = CostCenterFontSize;

        ws.Cells[row, 3].Value = (double)cc.SumCostCenter;
        ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00 €";

        using (var range = ws.Cells[row, 1, row, 3])
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml(CostCenterRowColor));
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        row += 2;
    }

    private static void WriteCategoryRow(ExcelWorksheet ws, ref int row, BudgetCategoryDto cat)
    {
        ws.Cells[row, 1].Value = cat.CategoryName;
        ws.Cells[row, 1].Style.Font.Bold = true;
        ws.Cells[row, 1].Style.Font.Size = CategoryFontSize;
        ws.Cells[row, 1].Style.Indent = 1;

        ws.Cells[row, 3].Value = (double)cat.SumCategories;
        ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00 €";

        using (var range = ws.Cells[row, 1, row, 3])
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml(CategoryRowColor));
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
        }

        row++;
    }

    private static void WriteItemDetailRows(ExcelWorksheet ws, ref int row, ref int detailRowCounter, BudgetItemDetailDto item)
    {
        var personSum = item.Persons.Sum(p => p.SumPerson);

        var showItemRow =
            !string.IsNullOrWhiteSpace(item.ItemDetailName) ||
            item.Persons.Count == 0 ||
            personSum != item.SumItemDetails;

        if (showItemRow)
        {
            detailRowCounter++;
            var altBackground = detailRowCounter % 2 == 0;

            ws.Cells[row, 2].Value = item.ItemDetailName;

            using (var range = ws.Cells[row, 2, row, 3])
            {
                if (altBackground)
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml(AlternatingRowColor));
                }
            }

            ws.Cells[row, 3].Value = (double)item.SumItemDetails;
            ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00 €";

            row++;
        }

        foreach (var p in item.Persons)
        {
            ws.Cells[row, 2].Value = $"      {p.PersonName}";
            ws.Cells[row, 2].Style.Font.Italic = true;

            ws.Cells[row, 3].Value = (double)p.SumPerson;
            ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00 €";

            ws.Cells[row, 2].Style.Indent = 2;

            row++;
        }
    }
}
