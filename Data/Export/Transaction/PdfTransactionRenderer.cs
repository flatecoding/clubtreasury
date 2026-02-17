using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TTCCashRegister.Data.Transaction;

namespace TTCCashRegister.Data.Export.Transaction;

public class PdfTransactionRenderer(ILogger<PdfTransactionRenderer> logger, IStringLocalizer<Translation> localizer) : IPdfTransactionRenderer
{
    public async Task RenderTransactionPdfExportAsync(IEnumerable<TransactionModel> transactions, DateTime begin, DateTime end,
        string filePath, string cashRegisterName, byte[]? logoData, string? logoContentType, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Rendering Transactions PDF → {File}", filePath);
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                logger.LogError("Directory does not exist: {Directory}", Path.GetDirectoryName(filePath));
                throw new DirectoryNotFoundException($"Directory does not exist: {directory}");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                RenderPdfSync(transactions, begin, end, filePath, cashRegisterName, logoData, logoContentType);
            }, cancellationToken);
            
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                throw new IOException($"PDF creation failed: {filePath} is empty or does not exist");
            }
            
            logger.LogInformation("PDF created → {File}", filePath);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("PDF rendering canceled → {File}", filePath);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while rendering pdf-file: {File}", filePath);
            throw;
        }
    }

    private void RenderPdfSync(IEnumerable<TransactionModel> transactions, DateTime begin, DateTime end, string filePath, string cashRegisterName, byte[]? logoData, string? logoContentType)
    {
        var ordered = transactions.OrderBy(t => t.Documentnumber).ToList();
        using var fs = new FileStream(filePath, FileMode.Create);
        Document.Create(container =>
            container.Page(page =>
            {
                page.Margin(10, Unit.Millimetre);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));
                page.Header().Element(lContainer => ComposeHeader(lContainer, begin, end, cashRegisterName, logoData, logoContentType));
                page.Content().Element(c => ComposeContent(c, ordered, begin, end));
                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span($"{localizer["Page"]}");
                        x.Span(" ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
            })).GeneratePdf(fs);
        fs.Flush();
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

    private void ComposeHeader(IContainer container, DateTime begin, DateTime end, string cashRegisterName, byte[]? logoData, string? logoContentType)
    {
        var isSvg = logoContentType?.Contains("svg", StringComparison.OrdinalIgnoreCase) == true;

        container.Row(row =>
        {
            if (logoData != null) RenderLogo(row.ConstantItem(100).AlignRight().Height(70), logoData, isSvg);
            row.RelativeItem()
                .AlignMiddle()
                .Column(column =>
                {
                    column.Item()
                        .Text(localizer["CashBook"] + " " + cashRegisterName)
                        .FontSize(20)
                        .Bold()
                        .AlignCenter();
                    column.Item()
                        .Text($"{begin.ToString("d", CultureInfo.CurrentUICulture)} - " +
                              $"{end.ToString("d", CultureInfo.CurrentUICulture)}")
                        .AlignCenter();
                    column.Item().Height(5);

                });
            if (logoData != null) RenderLogo(row.ConstantItem(100).AlignLeft().Height(70), logoData, isSvg);
        });
    }

    private static void RenderLogo(IContainer container, byte[] logoData, bool isSvg)
    {
        if (isSvg)
            container.Svg(InlineSvgStyles(System.Text.Encoding.UTF8.GetString(logoData)));
        else
            container.Image(logoData);
    }

    private static string InlineSvgStyles(string svg)
    {
        var styleMatch = Regex.Match(svg, @"<style[^>]*>\s*(?:<!\[CDATA\[)?(.*?)(?:\]\]>)?\s*</style>", RegexOptions.Singleline);
        if (!styleMatch.Success)
            return svg;

        var rules = new Dictionary<string, string>();
        foreach (Match rule in Regex.Matches(styleMatch.Groups[1].Value, @"\.(\w+)\s*\{([^}]+)\}"))
        {
            rules[rule.Groups[1].Value] = rule.Groups[2].Value.Trim();
        }

        var result = Regex.Replace(svg, @"class=""([^""]+)""", match =>
        {
            var classNames = match.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var inlineStyles = classNames
                .Where(c => rules.ContainsKey(c))
                .Select(c => rules[c])
                .ToList();

            return inlineStyles.Count > 0
                ? $"style=\"{string.Join(";", inlineStyles)}\""
                : match.Value;
        });

        result = Regex.Replace(result, @"<style[^>]*>.*?</style>", "", RegexOptions.Singleline);
        return result;
    }
    
    private void ComposeContent(IContainer container, List<TransactionModel> ordered, DateTime begin, DateTime end)
    {
        const string headerBackgroundColor = "#DDEBF7";
        var headerStyle = TextStyle.Default.SemiBold();
        var culture = CultureInfo.CurrentUICulture;

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
                    header.Cell().Element(CellHeaderStyle).Text($"{localizer["Date"]}").Style(headerStyle).AlignCenter();
                    header.Cell().Element(CellHeaderStyle).Text($"{localizer["DocumentNumberShort"]}").Style(headerStyle).AlignCenter();
                    header.Cell().Element(CellHeaderStyle).Text($"{localizer["Description"]}").Style(headerStyle);
                    header.Cell().Element(CellHeaderStyle).Text($"{localizer["Sum"]}").Style(headerStyle).AlignCenter();
                    header.Cell().Element(CellHeaderStyle).Text($"{localizer["Account"]}").Style(headerStyle).AlignCenter();

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
            row.RelativeItem().Text("");

            row.ConstantItem(200).Column(sumCol =>
            {
                sumCol.Item().Text($"{localizer["TotalIncome"]}:  {totalIncome.ToString("C2", culture)}")
                    .Style(TextStyle.Default.SemiBold())
                    .FontColor(Colors.Green.Darken2)
                    .AlignRight();

                sumCol.Item().Text($"{localizer["TotalExpenses"]}:  {totalExpense.ToString("C2", culture)}")
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
            row.ConstantItem(100).AlignMiddle().Text($"{localizer["AccountBalance"]} {begin.ToString("d", culture)}:");
            row.ConstantItem(100).AlignBottom().LineHorizontal(1)
                .LineColor(Colors.Grey.Lighten1);
        });

        col.Item().PaddingTop(10).Row(row =>
        {
            row.ConstantItem(100).AlignMiddle().Text($"{localizer["AccountBalance"]} {end.ToString("d", culture)}:");
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
