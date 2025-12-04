using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TTCCashRegister.Data.Transaction;

namespace TTCCashRegister.Data.Export.Transaction;

public class PdfTransactionRenderer(ILogger<PdfTransactionRenderer> logger) : IPdfTransactionRenderer
{
    private const string LogoFileName = "ttc_logo.png";
    private readonly string _logoRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
    private byte[]? _imageData;
    

    public async Task RenderTransactionPdfExportAsync(IEnumerable<TransactionModel> transactions, DateTime begin, DateTime end,
        string filePath)
    {
        try
        {
            logger.LogInformation("Rendering Transactions PDF → {File}", filePath);
            _imageData = await GetImageData();

            var ordered = transactions.OrderBy(t => t.Documentnumber).ToList();

            Document.Create(container =>
                container.Page(page =>
                {
                    page.Margin(10, Unit.Millimetre);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));
                    page.Header().Element(lContainer => ComposeHeader(lContainer, begin, end));
                    page.Content().Element(c => ComposeContent(c, ordered));
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Seite ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                })).GeneratePdf(filePath);
            logger.LogInformation("PDF erstellt → {File}", filePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler beim Rendern der PDF: {File}", filePath);
        }
    }

    
    static IContainer DefaultCellStyle(IContainer container, string backgroundColor)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten1)
            .Background(backgroundColor)
            .PaddingVertical(5)
            .PaddingHorizontal(2);
    }

    private void ComposeHeader(IContainer container, DateTime begin, DateTime end)
    {
        container.Row(row =>
        {
            if (_imageData != null) row.ConstantItem(100).AlignRight().Height(70).Image(_imageData);
            row.RelativeItem()
                .AlignMiddle()
                .Column(column =>
                {
                    column.Item()
                        .Text("Kassenbuch TTC Hagen e.V.")
                        .FontSize(20)
                        .Bold()
                        .AlignCenter();
                    column.Item()
                        .Text($"{begin:dd.MM.yyyy} - {end:dd.MM.yyyy}")
                        .AlignCenter();
                    column.Item().Height(5);

                });
            if (_imageData != null) row.ConstantItem(100).AlignLeft().Height(70).Image(_imageData);
        });
    }

    private async Task<byte[]?> GetImageData()
    {
        var logoPath = Path.Combine(_logoRoot, LogoFileName);
        byte[]? imageData = null;
        if (File.Exists(logoPath))
        {
            imageData = await File.ReadAllBytesAsync(logoPath);
        }else
        {
            logger.LogInformation("Image noch found '{LogoPath}'", logoPath);
        }
        return imageData;
    }
    
    private static void ComposeContent(IContainer container, List<TransactionModel> ordered)
    {
        const string headerBackgroundColor = "#DDEBF7";
    var headerStyle = TextStyle.Default.SemiBold();
    var culture = new CultureInfo("de-DE");

    container.PaddingTop(5, Unit.Millimetre).Column(col =>
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(80);  
                columns.ConstantColumn(60);  
                columns.RelativeColumn();    
                columns.ConstantColumn(70);  
                columns.ConstantColumn(70);  
            });

            table.Header(header =>
            {
                header.Cell().Element(CellHeaderStyle).Text("Datum").Style(headerStyle).AlignCenter();
                header.Cell().Element(CellHeaderStyle).Text("Belegnr.").Style(headerStyle).AlignCenter();
                header.Cell().Element(CellHeaderStyle).Text("Beschreibung").Style(headerStyle);
                header.Cell().Element(CellHeaderStyle).Text("Summe").Style(headerStyle).AlignCenter();
                header.Cell().Element(CellHeaderStyle).Text("Konto").Style(headerStyle).AlignCenter();

                IContainer CellHeaderStyle(IContainer containerHeader)
                    => DefaultCellStyle(containerHeader, headerBackgroundColor);
            });

            var index = 0;
            foreach (var t in ordered)
            {
                var background = GetRowBackgroundColor(index);

                table.Cell().Element(c => DefaultCellStyle(c, background)).Text(t.Date.ToString()).AlignCenter();
                table.Cell().Element(c => DefaultCellStyle(c, background)).Text($"B{t.Documentnumber}").AlignCenter();
                table.Cell().Element(c => DefaultCellStyle(c, background)).Text(t.Description);

                table.Cell().Element(c => DefaultCellStyle(c, background))
                    .Text(t.Sum.ToString("C2", culture))
                    .AlignRight();

                var textColor = t.AccountMovement >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;

                table.Cell().Element(c => DefaultCellStyle(c, background))
                    .Text(t.AccountMovement.ToString("C2", culture))
                    .Style(TextStyle.Default.FontColor(textColor))
                    .AlignRight();

                index++;
            }
        });
        
        var totalIncome = ordered.Where(x => x.AccountMovement > 0).Sum(x => x.AccountMovement);
        var totalExpense = ordered.Where(x => x.AccountMovement < 0).Sum(x => x.AccountMovement);
        var balance = ordered.Sum(x => x.AccountMovement);

        col.Item().PaddingTop(10).Row(row =>
        {
            row.RelativeItem().Text(""); // Platzhalter links

            row.ConstantItem(200).Column(sumCol =>
            {
                sumCol.Item().Text($"Gesamteinnahmen:  {totalIncome.ToString("C2", culture)}")
                    .Style(TextStyle.Default.SemiBold())
                    .FontColor(Colors.Green.Darken2)
                    .AlignRight();

                sumCol.Item().Text($"Gesamtausgaben:  {totalExpense.ToString("C2", culture)}")
                    .Style(TextStyle.Default.SemiBold())
                    .FontColor(Colors.Red.Darken2)
                    .AlignRight();

                sumCol.Item().PaddingTop(5)
                    .Text($"Saldo:  {balance.ToString("C2", culture)}")
                    .Style(TextStyle.Default.SemiBold())
                    .FontColor(balance >= 0 ? Colors.Green.Darken3 : Colors.Red.Darken3)
                    .AlignRight();
            });
        });
        col.Item().Row(row =>
        {
            row.ConstantItem(100).AlignMiddle().Text("Kontostand 01.01:");
            row.ConstantItem(100).AlignBottom().LineHorizontal(1)
                .LineColor(Colors.Grey.Lighten1);
        });
            
        col.Item().PaddingTop(10).Row(row =>
        {
            row.ConstantItem(100).AlignMiddle().Text("Kontostand 31.12:");
            row.ConstantItem(100).AlignBottom().LineHorizontal(1)
                .LineColor(Colors.Grey.Lighten1);
        });
    });
    }
    
    
    private static string GetRowBackgroundColor(int rowIndex)
    {
        return rowIndex % 2 == 0
            ? Colors.White
            : Colors.Grey.Lighten3;
    }
}
