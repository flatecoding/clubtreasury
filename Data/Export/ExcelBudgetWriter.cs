using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using TTCCashRegister.Data.Mapper.DTOs;

namespace TTCCashRegister.Data.Export;

public class ExcelBudgetWriter : IExcelBudgetWriter
{
    public async Task WriteAsync(string filePath, IEnumerable<BudgetGroupedDto> grouped, DateTime begin, DateTime end)
    {
        ExcelPackage.License.SetNonCommercialOrganization("TTC Hagen e.V.");

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Budget");

        // Titel
        ws.Cells[1, 1].Value = "Budget-Auswertung";
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.Font.Size = 18;

        ws.Cells[2, 1].Value = $"Zeitraum: {begin:dd.MM.yyyy} - {end:dd.MM.yyyy}";
        ws.Cells[2, 1].Style.Font.Italic = true;
        ws.Cells[2, 1].Style.Font.Size = 12;

        // Header
        int row = 4;
        ws.Cells[row, 1].Value = "Kostenstelle";
        ws.Cells[row, 2].Value = "Kategorie";
        ws.Cells[row, 3].Value = "Details / Person";
        ws.Cells[row, 4].Value = "Betrag";

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

        // -----------------------------------------------
        // Hierarchische Ausgabe mit verbesserter Formatierung
        // -----------------------------------------------
        foreach (var cc in grouped)
        {
            // ---------------- Kostenstelle ----------------
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

            row += 2; // Abstand zur Kategorie

            foreach (var cat in cc.Categories)
            {
                // ---------------- Kategorie ----------------
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

                    // ---------------- Personen ----------------
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

                row++; // Abstand nach Kategorie-Gruppe
            }
        }

        ws.Cells.AutoFitColumns();

        await package.SaveAsAsync(new FileInfo(filePath));
    }
}
