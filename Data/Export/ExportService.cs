using System.Diagnostics;
using System.Globalization;
using System.Text;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.Transaction;
using Path = System.IO.Path;


namespace TTCCashRegister.Data.Export
{
    public class ExportService
    {
        private readonly CashDataContext _context;
        private readonly string _exportPath;
        private const string SelectedFolder = "Export";
        private const string CsvHeader = "Belegnr.;Beschreibung;Rechnungsbetrag;Kontobewegung";
        private const string CsvBudgetHeader = "Kostenstelle;Position;Details;Summe";
        private const string Image = "Logo TTC Hagen.bmp";
        private const string PdfTitle = "Kassenbuch TTC Hagen";
        private const string Range = "Zeitraum";
        private const string PdfHeaderDate = "Datum";
        private const string PdfHeaderDocumentNumber = "Belegnr.";
        private const string PdfHeaderDescription = "Beschreibung";
        private const string PdfHeaderSum = "Summe";
        private const string PdfHeaderAccountMovement = "Konto";

        public ExportService(CashDataContext context)
        {
            _context = context;
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
            return await _context.Transactions
                .Include(t => t.CostUnit)
                .Include(t => t.BasicUnit)
                .ThenInclude(bu => bu.CostUnitDetails)
                .Where(t => t.Date >= DateOnly.FromDateTime(begin) && t.Date <= DateOnly.FromDateTime(end))
                .ToListAsync();
        }

       public async Task<bool> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename)
        {
            try
            {
                if (!Directory.Exists(_exportPath))
                {
                    Directory.CreateDirectory(_exportPath);
                }
                var transactions = await GetTransactionsInDateRange(begin, end);
                transactions = transactions.OrderBy(t => t.Documentnumber).ToList();

                var csv = new StringBuilder();
                foreach (var transaction in transactions)
                {
                    csv.AppendLine($"{transaction.Documentnumber};{transaction.Description};{transaction.Sum};{transaction.AccountMovement}");
                }

                if (string.IsNullOrWhiteSpace(csv.ToString()))
                {
                    Console.WriteLine("No data for export available.");
                    return false;
                }
                
                await using var sw = new StreamWriter(Path.Combine(_exportPath, filename));
                await sw.WriteLineAsync(CsvHeader);
                await sw.WriteAsync(csv);
                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return false;
            }
        }

        public async Task<bool> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename)
        {
            try
            {
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
                document.SetMargins(20, 30, 20, 30);
                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
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
                    var imageCell = new Cell(2,1).Add(img)
                        .SetBorder(Border.NO_BORDER);
                    headerTable.AddCell(imageCell);
                    
                    var title = new Paragraph(PdfTitle)
                        .SetFont(bold)
                        .SetFontSize(20)
                        .SetTextAlignment(TextAlignment.CENTER);
                    var titleCell = new Cell().Add(title).SetBorder(Border.NO_BORDER);
                    headerTable.AddCell(titleCell);
                    
                    var imageRightSide = new  Cell(2,1).Add(img)
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
                    Color rowColor = i % 2 == 0 ? PdfColors.RowColorEven: PdfColors.RowColorOdd;
                    var accColor = transaction.AccountMovement > 0 ? PdfColors.PositiveSum 
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
                document.Close();
                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return false;
            }
        }
        
        public async Task<bool> ExportBudgetToCsv(DateTime begin, DateTime end, string filename)
        {
            try
            {
                var folderPath = Path.Combine(_exportPath, SelectedFolder);
                var fullPath = Path.Combine(folderPath, filename);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var transactions = await GetBudgetByDateRange(begin, end);

                var groupedTransactions = 
                    transactions
                    .GroupBy(t => t.CostUnit)
                    .Select(costUnitGroup => new
                    {
                        CostUnit = costUnitGroup.Key,
                        SummeCostUnit = costUnitGroup.Sum(t => t.AccountMovement),
                        BasicUnits = costUnitGroup
                        .GroupBy(t => t.BasicUnit)
                        .Select(basicUnitGroup => new 
                        {
                            BasicUnit = basicUnitGroup.Key,
                            SummeBasicUnit = basicUnitGroup.Sum(t => t.AccountMovement),
                            UnitDetails = basicUnitGroup
                                            .GroupBy(t => t.UnitDetails)
                                            .Select(unitDetailGroup => new
                                            {
                                                UnitDetail = unitDetailGroup.Key,
                                                SummeUnitDetails = unitDetailGroup.Sum(t => t.AccountMovement)
                                            }).ToList()
                        }).ToList()
                    }).ToList();

                var csv = new StringBuilder();
                csv.AppendLine(CsvBudgetHeader);

                foreach (var costUnitGroup in groupedTransactions)
                {   
                    csv.AppendLine($"{costUnitGroup.CostUnit.CostUnitName};;;{costUnitGroup.SummeCostUnit.ToString("C", CultureInfo.CurrentCulture)}");
                    foreach (var basicUnitGroup in costUnitGroup.BasicUnits)
                    {
                        csv.AppendLine($";{basicUnitGroup.BasicUnit?.Name};;{basicUnitGroup.SummeBasicUnit.ToString("C", CultureInfo.CurrentCulture)}");
                        foreach (var unitDetailGroup in basicUnitGroup.UnitDetails)
                        {
                            var unitDetailName = unitDetailGroup.UnitDetail is not null ? unitDetailGroup.UnitDetail.CostDetails : "";
                            csv.AppendLine($";;{unitDetailName};{unitDetailGroup.SummeUnitDetails.ToString("C", CultureInfo.CurrentCulture)}");
                        }
                    }
                }

                await using var sw = new StreamWriter(fullPath);
                await sw.WriteAsync(csv.ToString());

                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return false;
            }
        }
    }
}

