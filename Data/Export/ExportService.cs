using System.Globalization;
using System.Text;
using AutoMapper;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using TTCCashRegister.Data.Mapper;
using TTCCashRegister.Data.Mapper.DTOs;
using TTCCashRegister.Data.Transaction;
using Path = System.IO.Path;
using Border = iText.Layout.Borders.Border;
using Cell = iText.Layout.Element.Cell;
using Color = iText.Kernel.Colors.Color;
using Table = iText.Layout.Element.Table;

namespace TTCCashRegister.Data.Export
{
    public class ExportService :IExportService
    {
        private readonly CashDataContext _context;
        private readonly ILogger<ExportService> _logger;
        private readonly IBudgetMapper _mapper;
        private readonly string _exportPath;
        private const string SelectedFolder = "Export";
        private const string CsvHeader = "Belegnr.;Beschreibung;Rechnungsbetrag;Kontobewegung";
        private const string CsvBudgetHeader = "Kostenstelle;Details/Person;Details;Summe";
        private const string Image = "Logo TTC Hagen.bmp";
        private const string PdfTitle = "Kassenbuch TTC Hagen";
        private const string Range = "Zeitraum";
        private const string PdfHeaderDate = "Datum";
        private const string PdfHeaderDocumentNumber = "Belegnr.";
        private const string PdfHeaderDescription = "Beschreibung";
        private const string PdfHeaderSum = "Summe";
        private const string PdfHeaderAccountMovement = "Konto";

        public ExportService(CashDataContext context, ILogger<ExportService> logger, IBudgetMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            
            var projectDirectory = AppContext.BaseDirectory;
            var binDirectory = Directory.GetParent(projectDirectory)?.Parent?.Parent?.FullName;
            if (binDirectory is null)
                throw new DirectoryNotFoundException($"The bin directory could not be found: {projectDirectory}");
            _exportPath = Path.Combine(binDirectory, SelectedFolder);
        }

        private async Task<List<TransactionModel>> GetTransactionsInDateRange(DateTime begin, DateTime end)
        {
            return await _context.Transactions
                .Where(t => t.Date >= DateOnly.FromDateTime(begin) && t.Date <= DateOnly.FromDateTime(end))
                .Select(t => new TransactionModel
                {
                    Date = t.Date,
                    Documentnumber = t.Documentnumber,
                    Description = t.Description,
                    Sum = t.Sum,
                    AccountMovement = t.AccountMovement
                }).ToListAsync();
        }

        private async Task<List<TransactionModel>> GetBudgetByDateRange(DateTime begin, DateTime end)
        {
            var beginDateOnly = DateOnly.FromDateTime(begin);
            var endDateOnly   = DateOnly.FromDateTime(end);

            return await _context.Transactions
                .AsSplitQuery()
                .Include(t => t.Allocation).ThenInclude(a => a.CostCenter)
                .Include(t => t.Allocation).ThenInclude(a => a.Category)
                .Include(t => t.Allocation).ThenInclude(a => a.ItemDetail)
                .Include(t => t.TransactionDetails).ThenInclude(st => st.Person)
                .Where(t => t.Date >= beginDateOnly && t.Date <= endDateOnly)
                .ToListAsync();
        }


        public async Task<bool> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename)
        {
            try
            {
                _logger.LogInformation("Beginning export transactions to csv");
                if (!Directory.Exists(_exportPath))
                {
                    Directory.CreateDirectory(_exportPath);
                    _logger.LogInformation("Export directory created {path}", _exportPath);
                }

                var transactions = await GetTransactionsInDateRange(begin, end);
                transactions = transactions.OrderBy(t => t.Documentnumber).ToList();

                var csv = new StringBuilder();
                foreach (var transaction in transactions)
                {
                    csv.AppendLine(
                        $"{transaction.Documentnumber};{transaction.Description};{transaction.Sum};{transaction.AccountMovement}");
                }

                if (string.IsNullOrWhiteSpace(csv.ToString()))
                {
                    _logger.LogError("CSV file for transactions could not be created");
                    return false;
                }

                await using var sw = new StreamWriter(Path.Combine(_exportPath, filename));
                await sw.WriteLineAsync(CsvHeader);
                await sw.WriteAsync(csv);
                _logger.LogInformation("Export transactions to csv completed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while exporting transactions to csv");
                return false;
            }
        }

        public async Task<bool> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename)
        {
            try
            {
                _logger.LogInformation("Beginning export transactions to pdf");
                if (!Directory.Exists(_exportPath))
                {
                    Directory.CreateDirectory(_exportPath);
                }

                var transactions = await GetTransactionsInDateRange(begin, end);
                var orderedTransactions = transactions.OrderBy(t => t.Documentnumber).ToList();
                var writer = new PdfWriter(Path.Combine(_exportPath, filename));
                var pdf = new PdfDocument(writer);
                //var document = new Document(pdf, PageSize.A4.Rotate());
                var document = new Document(pdf, PageSize.A4);
                document.SetMargins(20, 30, 40, 30);
                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                // EventHandler registrieren
                var handler =
                    new PageNumberEventHandler(font, footerPageCounterBottomMargin: 20f, placeholderWidth: 50f);
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);

                var table = new Table(5);
                table.SetWidth(UnitValue.CreatePercentValue(100));

                //Add header
                var imagePath = $"{Directory.GetCurrentDirectory()}{@"/wwwroot/images"}";
                var fullFileName = Path.Combine(imagePath, Image);

                if (File.Exists(fullFileName))
                {
                    // Create a table with a row and two cells
                    var headerTable = new Table(3);
                    headerTable.SetWidth(UnitValue.CreatePercentValue(100));
                    //headerTable.SetBorder(Border.NO_BORDER);

                    // Add image in first row
                    var img = new Image(ImageDataFactory.Create(fullFileName));
                    img.ScaleToFit(60, 60);
                    var imageCell = new Cell(2, 1).Add(img)
                        .SetBorder(Border.NO_BORDER);
                    headerTable.AddCell(imageCell);

                    var title = new Paragraph(PdfTitle)
                        .SetFont(bold)
                        .SetFontSize(20)
                        .SetTextAlignment(TextAlignment.CENTER);
                    var titleCell = new Cell().Add(title).SetBorder(Border.NO_BORDER);
                    headerTable.AddCell(titleCell);

                    var imageRightSide = new Cell(2, 1).Add(img)
                        .SetBorder(Border.NO_BORDER)
                        .SetHorizontalAlignment(HorizontalAlignment.LEFT);
                    headerTable.AddCell(imageRightSide);

                    var period = new Paragraph($"{Range}: {begin:dd.MM.yyyy} - {end:dd.MM.yyyy}")
                        .SetFont(font)
                        .SetFontSize(14)
                        .SetTextAlignment(TextAlignment.CENTER);
                    var periodCell = new Cell().Add(period).SetBorder(Border.NO_BORDER);
                    headerTable.AddCell(periodCell);

                    document.Add(headerTable);
                }

                document.Add(new Paragraph("\n"));
                table.AddHeaderCell(new Cell().Add(new Paragraph(PdfHeaderDate).SetFont(bold))
                    .SetBackgroundColor(PdfColors.HeaderColor));
                table.AddHeaderCell(new Cell().Add(new Paragraph(PdfHeaderDocumentNumber).SetFont(bold))
                    .SetBackgroundColor(PdfColors.HeaderColor));
                table.AddHeaderCell(new Cell().Add(new Paragraph(PdfHeaderDescription).SetFont(bold))
                    .SetBackgroundColor(PdfColors.HeaderColor));
                table.AddHeaderCell(new Cell().Add(new Paragraph(PdfHeaderSum).SetFont(bold))
                    .SetBackgroundColor(PdfColors.HeaderColor));
                table.AddHeaderCell(new Cell().Add(new Paragraph(PdfHeaderAccountMovement).SetFont(bold))
                    .SetBackgroundColor(PdfColors.HeaderColor));

                for (var i = 0; i < orderedTransactions.Count; i++)
                {
                    var transaction = orderedTransactions[i];
                    Color rowColor = i % 2 == 0 ? PdfColors.RowColorEven : PdfColors.RowColorOdd;
                    var accColor = transaction.AccountMovement > 0
                        ? PdfColors.PositiveSum
                        : PdfColors.NegativeSum;

                    table.AddCell(new Cell()
                        .Add(new Paragraph(transaction.Date.ToString())
                            .SetFont(font))
                        .SetBackgroundColor(rowColor));
                    table.AddCell(new Cell()
                        .Add(new Paragraph(transaction.Documentnumber.ToString())
                            .SetFont(font)
                            .SetTextAlignment(TextAlignment.CENTER))
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                        .SetBackgroundColor(rowColor));
                    table.AddCell(new Cell()
                        .Add(new Paragraph(transaction.Description)
                            .SetFont(font))
                        .SetBackgroundColor(rowColor));
                    table.AddCell(new Cell()
                        .Add(new Paragraph(transaction.Sum.ToString(CultureInfo.CurrentCulture))
                            .SetFont(font))
                        .SetBackgroundColor(rowColor));
                    table.AddCell(new Cell().Add(
                            new Paragraph(transaction.AccountMovement.ToString(CultureInfo.CurrentCulture))
                                .SetFont(font)
                                .SetFontColor(accColor))
                        .SetBackgroundColor(rowColor));
                }

                document.Add(table);
                handler.WriteTotal(pdf);
                document.Close();
                _logger.LogInformation("Export transactions to pdf completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export transactions to pdf failed");
                return false;
            }
        }
        
        public async Task<bool> ExportBudgetToCsv(DateTime begin, DateTime end, string filename)
        {
            try
            {
                _logger.LogInformation("Export budget to csv started");

                if (!Directory.Exists(_exportPath))
                    Directory.CreateDirectory(_exportPath);

                var transactions = await GetBudgetByDateRange(begin, end);
                
                var flatEntries = new List<BudgetFlatEntryDto>();

                foreach (var t in transactions)
                {
                    if (!t.TransactionDetails.Any())
                        flatEntries.Add(_mapper.MapTransaction(t));

                    foreach (var td in t.TransactionDetails)
                        flatEntries.Add(_mapper.MapTransactionDetail(td));
                }

                // --- Gruppierung ---
                var grouped = flatEntries
                    .GroupBy(e => new { e.CostCenterId, e.CostCenterName })
                    .Select(costCenterGroup => new BudgetGroupedDto
                    {
                        CostCenterId = costCenterGroup.Key.CostCenterId,
                        CostUnitName = costCenterGroup.Key.CostCenterName,
                        SumCostCenter = costCenterGroup.Sum(e => e.Amount),

                        Categories = costCenterGroup
                            .GroupBy(e => new { e.CategoryId, e.CategoryName })
                            .Select(cat => new BudgetCategoryDto
                            {
                                CategoryId = cat.Key.CategoryId,
                                CategoryName = cat.Key.CategoryName,
                                SumCategories = cat.Sum(e => e.Amount),

                                ItemDetails = cat
                                    .GroupBy(e => new { e.ItemDetailId, e.ItemDetailName })
                                    .Select(item => new BudgetItemDetailDto
                                    {
                                        ItemDetailId = item.Key.ItemDetailId,
                                        ItemDetailName = item.Key.ItemDetailName,
                                        SumItemDetails = item.Sum(e => e.Amount),

                                        Persons = item
                                            .Where(p => p.PersonId != null)
                                            .GroupBy(p => new { p.PersonId, p.PersonName })
                                            .Select(p => new BudgetPersonDto
                                            {
                                                PersonId = p.Key.PersonId!.Value,
                                                PersonName = p.Key.PersonName!,
                                                SumPerson = p.Sum(x => x.Amount)
                                            })
                                            .OrderBy(x => x.PersonName)
                                            .ToList()
                                    })
                                    .OrderBy(i => i.ItemDetailName)
                                    .ToList()
                            })
                            .OrderBy(c => c.CategoryName)
                            .ToList()
                    })
                    .OrderBy(cc => cc.CostUnitName)
                    .ToList();

                // --- CSV erzeugen ---
                var csv = new StringBuilder();
                csv.AppendLine(CsvBudgetHeader);

                foreach (var costCenter in grouped)
                {
                    csv.AppendLine($"{costCenter.CostUnitName};;;{costCenter.SumCostCenter:C}");

                    foreach (var cat in costCenter.Categories)
                    {
                        csv.AppendLine($";{cat.CategoryName};;{cat.SumCategories:C}");

                        foreach (var item in cat.ItemDetails)
                        {
                            var personSum = item.Persons.Sum(p => p.SumPerson);

                            if (!string.IsNullOrWhiteSpace(item.ItemDetailName) ||
                                item.Persons.Count == 0 ||
                                personSum != item.SumItemDetails)
                            {
                                csv.AppendLine($";;{item.ItemDetailName};{item.SumItemDetails:C}");
                            }

                            foreach (var person in item.Persons)
                            {
                                csv.AppendLine($";;{person.PersonName};{person.SumPerson:C}");
                            }
                        }
                    }
                }

                await File.WriteAllTextAsync(Path.Combine(_exportPath, filename), csv.ToString());

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating CSV");
                return false;
            }
        }



        // -------------------------------------------------------
        // Excel Export mit Charts (angepasst auf manuelles Mapping)
        // -------------------------------------------------------
        public async Task<bool> ExportBudgetToExcelWithCharts(DateTime begin, DateTime end, string filename)
        {
            ExcelPackage.License.SetNonCommercialOrganization("TTC Hagen e.V.");

            try
            {
                _logger.LogInformation("Export budget to Excel started");

                if (!Directory.Exists(_exportPath))
                    Directory.CreateDirectory(_exportPath);

                var transactions = await GetBudgetByDateRange(begin, end);

                // --- MANUELLES MAPPING ---
                var flatEntries = new List<BudgetFlatEntryDto>();

                foreach (var t in transactions)
                {
                    if (!t.TransactionDetails.Any())
                        flatEntries.Add(_mapper.MapTransaction(t));

                    foreach (var td in t.TransactionDetails)
                        flatEntries.Add(_mapper.MapTransactionDetail(td));
                }

                // Gruppierung wie bisher, aber mit flatEntries statt dyn
                var grouped = flatEntries
                    .GroupBy(e => new { e.CostCenterId, e.CostCenterName })
                    .Select(costCenterGroup => new
                    {
                        CostCenterId = costCenterGroup.Key.CostCenterId,
                        CostUnitName = costCenterGroup.Key.CostCenterName,
                        SummeCostUnit = costCenterGroup.Sum(e => e.Amount),

                        Categories = costCenterGroup
                            .GroupBy(e => new { e.CategoryId, e.CategoryName })
                            .Select(categoryGroup => new
                            {
                                categoryGroup.Key.CategoryId,
                                CategoryName = categoryGroup.Key.CategoryName,
                                SumOfCategories = categoryGroup.Sum(e => e.Amount),

                                ItemDetails = categoryGroup
                                    .GroupBy(e => new { e.ItemDetailId, e.ItemDetailName })
                                    .Select(itemDetailGroup => new
                                    {
                                        itemDetailGroup.Key.ItemDetailId,
                                        itemDetailGroup.Key.ItemDetailName,
                                        SumOfItemDetails = itemDetailGroup.Sum(e => e.Amount),

                                        Persons = itemDetailGroup
                                            .Where(x => x.PersonId != null)
                                            .GroupBy(x => new { x.PersonId, x.PersonName })
                                            .Select(p => new
                                            {
                                                PersonId = p.Key.PersonId!,
                                                PersonName = p.Key.PersonName!,
                                                SummePerson = p.Sum(x => x.Amount)
                                            })
                                            .OrderBy(p => p.PersonName)
                                            .ToList()
                                    })
                                    .OrderBy(id => id.ItemDetailName)
                                    .ToList()
                            })
                            .OrderBy(cat => cat.CategoryName)
                            .ToList()
                    })
                    .OrderBy(cc => cc.CostUnitName)
                    .ToList();

                using var package = new ExcelPackage();

                CreateTransactionsSheetEpPlus(package, grouped, begin, end);
                CreateAnalysisSheetEpPlus(package, grouped);

                await package.SaveAsAsync(new FileInfo(Path.Combine(_exportPath, filename)));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export budget to Excel failed");
                return false;
            }
        }

        private void CreateTransactionsSheetEpPlus(ExcelPackage package, dynamic grouped, DateTime begin, DateTime end)
        {
            var workSheet = package.Workbook.Worksheets.Add("Transaktionen");

            // Titel
            workSheet.Cells[1, 1].Value = "Budget-Auswertung";
            workSheet.Cells[1, 1].Style.Font.Bold = true;
            workSheet.Cells[1, 1].Style.Font.Size = 16;

            workSheet.Cells[2, 1].Value = $"Zeitraum: {begin:dd.MM.yyyy} - {end:dd.MM.yyyy}";
            workSheet.Cells[2, 1].Style.Font.Italic = true;

            // Header
            int currentRow = 4;
            workSheet.Cells[currentRow, 1].Value = "Kostenstelle";
            workSheet.Cells[currentRow, 2].Value = "Kategorie";
            workSheet.Cells[currentRow, 3].Value = "Details";
            workSheet.Cells[currentRow, 4].Value = "Betrag";

            // Header formatieren
            using (var range = workSheet.Cells[currentRow, 1, currentRow, 4])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGray);
                range.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            currentRow++;

            foreach (var costCenterGroup in grouped)
            {
                // Kostenstelle
                workSheet.Cells[currentRow, 1].Value = costCenterGroup.CostUnitName;
                workSheet.Cells[currentRow, 4].Value = costCenterGroup.SummeCostUnit;
                workSheet.Cells[currentRow, 4].Style.Numberformat.Format = "#,##0.00 €";

                using (var range = workSheet.Cells[currentRow, 1, currentRow, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                currentRow++;

                foreach (var categoryGroup in costCenterGroup.Categories)
                {
                    workSheet.Cells[currentRow, 2].Value = categoryGroup.CategoryName;
                    workSheet.Cells[currentRow, 4].Value = categoryGroup.SumOfCategories;
                    workSheet.Cells[currentRow, 4].Style.Numberformat.Format = "#,##0.00 €";

                    using (var range = workSheet.Cells[currentRow, 2, currentRow, 4])
                    {
                        range.Style.Font.Bold = true;
                    }

                    currentRow++;

                    foreach (var itemDetailGroup in categoryGroup.ItemDetails)
                    {
                        var itemDetailName = itemDetailGroup.ItemDetailName;
                        decimal sumsOfPersons = 0;

                        foreach (var p in itemDetailGroup.Persons)
                        {
                            sumsOfPersons += (decimal)p.SummePerson;
                        }

                        if (!string.IsNullOrWhiteSpace(itemDetailName) ||
                            itemDetailGroup.Persons.Count == 0 ||
                            sumsOfPersons != itemDetailGroup.SumOfItemDetails)
                        {
                            workSheet.Cells[currentRow, 3].Value = itemDetailName;
                            workSheet.Cells[currentRow, 4].Value = itemDetailGroup.SumOfItemDetails;
                            workSheet.Cells[currentRow, 4].Style.Numberformat.Format = "#,##0.00 €";
                            currentRow++;
                        }

                        foreach (var personGroup in itemDetailGroup.Persons)
                        {
                            workSheet.Cells[currentRow, 3].Value = $"  {personGroup.PersonName}";
                            workSheet.Cells[currentRow, 4].Value = personGroup.SummePerson;
                            workSheet.Cells[currentRow, 4].Style.Numberformat.Format = "#,##0.00 €";
                            workSheet.Cells[currentRow, 3].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            currentRow++;
                        }
                    }
                }
            }

            workSheet.Cells.AutoFitColumns();
        }

        private void CreateAnalysisSheetEpPlus(ExcelPackage package, dynamic grouped)
        {
            var ws = package.Workbook.Worksheets.Add("Auswertungen");

            // Titel
            ws.Cells[1, 1].Value = "Übersicht nach Kostenstellen";
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;

            // Header
            int currentRow = 3;
            ws.Cells[currentRow, 1].Value = "Kostenstelle";
            ws.Cells[currentRow, 2].Value = "Gesamt";
            ws.Cells[currentRow, 3].Value = "Einnahmen";
            ws.Cells[currentRow, 4].Value = "Ausgaben";

            using (var range = ws.Cells[currentRow, 1, currentRow, 4])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(217, 225, 242));
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            int dataStartRow = currentRow + 1;
            currentRow++;

            foreach (var costCenter in grouped)
            {
                decimal gesamt = costCenter.SummeCostUnit;

                decimal einnahmen = gesamt > 0 ? gesamt : 0;
                decimal ausgaben = gesamt < 0 ? Math.Abs(gesamt) : 0;

                if (einnahmen == 0 && ausgaben == 0)
                    ausgaben = 0.0001m;

                ws.Cells[currentRow, 1].Value = costCenter.CostUnitName;
                ws.Cells[currentRow, 2].Value = (double)gesamt;
                ws.Cells[currentRow, 3].Value = (double)einnahmen;
                ws.Cells[currentRow, 4].Value = (double)ausgaben;
                
                ws.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00 €";
                ws.Cells[currentRow, 3].Style.Numberformat.Format = "#,##0.00 €";
                ws.Cells[currentRow, 4].Style.Numberformat.Format = "#,##0.00 €";

                currentRow++;
            }

            int dataEndRow = currentRow - 1;
            if(dataEndRow < dataStartRow)
                return;

            // Gesamtsummen
            ws.Cells[currentRow, 1].Value = "GESAMT";
            ws.Cells[currentRow, 1].Style.Font.Bold = true;

            for (int col = 2; col <= 4; col++)
            {
                ws.Cells[currentRow, col].Formula =
                    $"=SUM({ws.Cells[dataStartRow, col].Address}:{ws.Cells[dataEndRow, col].Address})";
                ws.Cells[currentRow, col].Style.Numberformat.Format = "#,##0.00 €";
                ws.Cells[currentRow, col].Style.Font.Bold = true;
            }

            // Spaltenbreite
            ws.Column(1).Width = 30;
            ws.Column(2).Width = 15;
            ws.Column(3).Width = 15;
            ws.Column(4).Width = 15;

            // Diagramm 1: Säulendiagramm
            var chart1 = ws.Drawings.AddChart("ChartEinnahmenAusgaben", eChartType.ColumnClustered);
            chart1.Title.Text = "Einnahmen und Ausgaben nach Kostenstellen";
            chart1.SetPosition(2, 0, 5, 0);
            chart1.SetSize(600, 400);

            // Kategorien (X-Achse)
            var categories = ws.Cells[dataStartRow, 1, dataEndRow, 1];

            // Einnahmen Serie
            var seriesEinnahmen = chart1.Series.Add(ws.Cells[dataStartRow, 3, dataEndRow, 3], categories);
            seriesEinnahmen.Header = "Einnahmen";

            // Ausgaben Serie
            var seriesAusgaben = chart1.Series.Add(ws.Cells[dataStartRow, 4, dataEndRow, 4], categories);
            seriesAusgaben.Header = "Ausgaben";

            // Diagramm 2: Kreisdiagramm für Ausgaben
            int pieChartRow = currentRow + 5;
            ws.Cells[pieChartRow, 1].Value = "Ausgaben-Verteilung";
            ws.Cells[pieChartRow, 1].Style.Font.Bold = true;
            ws.Cells[pieChartRow, 1].Style.Font.Size = 12;

            pieChartRow += 2;
            ws.Cells[pieChartRow, 1].Value = "Kostenstelle";
            ws.Cells[pieChartRow, 2].Value = "Ausgaben";

            using (var range = ws.Cells[pieChartRow, 1, pieChartRow, 2])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(217, 225, 242));
            }

            int pieDataStartRow = pieChartRow + 1;
            pieChartRow++;

            foreach (var costCenter in grouped)
            {
                if (costCenter.SummeCostUnit < 0)
                {
                    ws.Cells[pieChartRow, 1].Value = costCenter.CostUnitName;
                    ws.Cells[pieChartRow, 2].Value = Math.Abs(costCenter.SummeCostUnit);
                    ws.Cells[pieChartRow, 2].Style.Numberformat.Format = "#,##0.00 €";
                    pieChartRow++;
                }
            }

            int pieDataEndRow = pieChartRow - 1;

            if (pieDataEndRow >= pieDataStartRow)
            {
                var chart2 = ws.Drawings.AddChart("ChartAusgabenVerteilung", eChartType.Pie);
                chart2.Title.Text = "Verteilung der Ausgaben";
                chart2.SetPosition(pieDataStartRow - 3, 0, 5, 0);
                chart2.SetSize(600, 400);

                var pieCategories = ws.Cells[pieDataStartRow, 1, pieDataEndRow, 1];
                var pieValues = ws.Cells[pieDataStartRow, 2, pieDataEndRow, 2];

                var pieSeries = chart2.Series.Add(pieValues, pieCategories);
                pieSeries.Header = "Ausgaben";
            }

            // Top Kategorien
            int categoryRow = pieChartRow + 3;
            ws.Cells[categoryRow, 1].Value = "Top Kategorien nach Ausgaben";
            ws.Cells[categoryRow, 1].Style.Font.Bold = true;
            ws.Cells[categoryRow, 1].Style.Font.Size = 12;

            categoryRow += 2;
            ws.Cells[categoryRow, 1].Value = "Kategorie";
            ws.Cells[categoryRow, 2].Value = "Ausgaben";

            using (var range = ws.Cells[categoryRow, 1, categoryRow, 2])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(217, 225, 242));
            }

            categoryRow++;

            var categoryTotals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (var costCenter in grouped)
            {
                foreach (var category in costCenter.Categories)
                {
                    string name = category.CategoryName;
                    decimal amount = (decimal)category.SumOfCategories;

                    if (categoryTotals.ContainsKey(name))
                        categoryTotals[name] += amount;
                    else
                        categoryTotals[name] = amount;
                }
            }

            // Nur negative Werte (Ausgaben)
            var allCategories = categoryTotals
                .Where(ct => ct.Value < 0)
                .OrderBy(ct => ct.Value)
                .Take(10)
                .Select(ct => new
                {
                    CategoryName = ct.Key,
                    TotalAmount = ct.Value
                })
                .ToList();

            foreach (var category in allCategories)
            {
                ws.Cells[categoryRow, 1].Value = category.CategoryName;
                ws.Cells[categoryRow, 2].Value = Math.Abs(category.TotalAmount);
                ws.Cells[categoryRow, 2].Style.Numberformat.Format = "#,##0.00 €";
                categoryRow++;
            }
        }

// Optional: Für Download in Blazor
        public async Task<byte[]> ExportBudgetToExcelBytes(DateTime begin, DateTime end)
        {
            var filename = $"Budget_{begin:yyyyMMdd}_{end:yyyyMMdd}.xlsx";
            var success = await ExportBudgetToExcelWithCharts(begin, end, filename);

            if (success)
            {
                var filePath = Path.Combine(_exportPath, filename);
                return await File.ReadAllBytesAsync(filePath);
            }

            return Array.Empty<byte>();
        }
    }
}


