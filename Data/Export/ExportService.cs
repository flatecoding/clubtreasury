using System.Globalization;
using System.Text;
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
using TTCCashRegister.Data.Mapper;
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
        private readonly IBudgetMapper _budgetMapper;
        private readonly ICsvBudgetWriter _csvWriter;
        private readonly IExcelBudgetWriter _excelWriter;
        private readonly string _exportPath;
        private const string SelectedFolder = "Export";
        private const string CsvHeader = "Belegnr.;Beschreibung;Rechnungsbetrag;Kontobewegung";
        private const string Image = "Logo TTC Hagen.bmp";
        private const string PdfTitle = "Kassenbuch TTC Hagen";
        private const string Range = "Zeitraum";
        private const string PdfHeaderDate = "Datum";
        private const string PdfHeaderDocumentNumber = "Belegnr.";
        private const string PdfHeaderDescription = "Beschreibung";
        private const string PdfHeaderSum = "Summe";
        private const string PdfHeaderAccountMovement = "Konto";

        public ExportService(CashDataContext context, ILogger<ExportService> logger, IBudgetMapper budgetMapper, 
            ICsvBudgetWriter csvWriter, IExcelBudgetWriter excelWriter)
        {
            _context = context;
            _logger = logger;
            _budgetMapper = budgetMapper;
            _csvWriter = csvWriter;
            _excelWriter = excelWriter;
            
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
                IPageNumberEventHandler handler =
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
                var flat = _budgetMapper.BuildFlatEntries(transactions);
                var grouped = _budgetMapper.BuildBudgetHierarchy(flat);

                var filePath = Path.Combine(_exportPath, filename);
                await _csvWriter.WriteAsync(filePath, grouped);
                _logger.LogInformation("Export budget to csv completed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating budget csv");
                return false;
            }
        }
        
        public async Task<bool> ExportBudgetToExcelWithCharts(DateTime begin, DateTime end, string filename)
        {
            try
            {
                _logger.LogInformation("Export budget to Excel started");

                if (!Directory.Exists(_exportPath))
                    Directory.CreateDirectory(_exportPath);

                var transactions = await GetBudgetByDateRange(begin, end);
                var flatEntries = _budgetMapper.BuildFlatEntries(transactions);
                var grouped = _budgetMapper.BuildBudgetHierarchy(flatEntries);

                var filePath = Path.Combine(_exportPath, filename);
                await _excelWriter.WriteAsync(filePath, grouped, begin, end);

                _logger.LogInformation("Export budget to Excel completed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export budget to Excel: Error creating Excel file");
                return false;
            }
        }
        
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


