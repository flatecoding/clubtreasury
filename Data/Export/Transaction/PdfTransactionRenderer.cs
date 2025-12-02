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
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));
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
        container
            .PaddingTop(5, Unit.Millimetre)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);  // Date
                    columns.ConstantColumn(60);  // Document number
                    columns.RelativeColumn();         // Description
                    columns.ConstantColumn(70);  // Sum
                    columns.ConstantColumn(70);  // Account
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
            foreach (var transaction in ordered)
            {
                var background = GetRowBackgroundColor(index);
                table.Cell().Element(c => DefaultCellStyle(c, background)).Text(transaction.Date.ToString()).AlignCenter();
                table.Cell().Element(c => DefaultCellStyle(c, background)).Text($"B{transaction.Documentnumber}").AlignCenter();
                table.Cell().Element(c => DefaultCellStyle(c, background)).Text(transaction.Description);

                table.Cell().Element(c => DefaultCellStyle(c, background))
                    .Text(transaction.Sum.ToString("C2", new CultureInfo("de-DE")))
                    .AlignRight();

                var textColor = transaction.AccountMovement >= 0
                    ? Colors.Green.Darken2
                    : Colors.Red.Darken2;

                table.Cell().Element(c => DefaultCellStyle(c, background))
                    .Text(transaction.AccountMovement.ToString("C2", new CultureInfo("de-DE")))
                    .Style(TextStyle.Default.FontColor(textColor))
                    .AlignRight();

                index++;
            }
            });
    }
    
    private static string GetRowBackgroundColor(int rowIndex)
    {
        return rowIndex % 2 == 0
            ? Colors.White
            : Colors.Grey.Lighten3;
    }
}
