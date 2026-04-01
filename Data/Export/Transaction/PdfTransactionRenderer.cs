using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Data.Export.Transaction;

public class PdfTransactionRenderer(ILogger<PdfTransactionRenderer> logger, IStringLocalizer<Translation> localizer) 
    : IPdfTransactionRenderer
{
    private const string HeaderBackgroundColor = "#DDEBF7";
    private const int DateColumnWidth = 80;
    private const int DocumentNumberColumnWidth = 60;
    private const int SumColumnWidth = 70;
    private const int AccountColumnWidth = 70;
    public async Task RenderTransactionPdfExportAsync(IEnumerable<TransactionModel> transactions, 
                                                       PdfRenderOptions options, CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Rendering Transactions PDF → {File}", options.FilePath);
            var directory = Path.GetDirectoryName(options.FilePath);
            if (!Directory.Exists(directory))
            {
                logger.LogError("Directory does not exist: {Directory}", Path.GetDirectoryName(options.FilePath));
                throw new DirectoryNotFoundException($"Directory does not exist: {directory}");
            }

            ct.ThrowIfCancellationRequested();

            await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                RenderPdfSync(transactions, options);
            }, ct);

            if (!File.Exists(options.FilePath) || new FileInfo(options.FilePath).Length == 0)
            {
                throw new IOException($"PDF creation failed: {options.FilePath} is empty or does not exist");
            }

            logger.LogInformation("PDF created → {File}", options.FilePath);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("PDF rendering canceled → {File}", options.FilePath);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while rendering pdf-file: {File}", options.FilePath);
            throw;
        }
    }

    private void RenderPdfSync(IEnumerable<TransactionModel> transactions, PdfRenderOptions options)
    {
        var ordered = transactions.OrderBy(t => t.Documentnumber).ToList();
        using var fs = new FileStream(options.FilePath, FileMode.Create);
        Document.Create(container =>
            container.Page(page =>
            {
                page.Margin(10, Unit.Millimetre);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));
                page.Header().Element(lContainer => ComposeHeader(lContainer, options));
                page.Content().Element(c => ComposeContent(c, ordered, options));
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

    private void ComposeHeader(IContainer container, PdfRenderOptions options)
    {
        var isSvg = options.LogoContentType?.Contains("svg", StringComparison.OrdinalIgnoreCase) == true;

        container.Row(row =>
        {
            if (options.LogoData != null) RenderLogo(row.ConstantItem(100).AlignRight().Height(70), 
                                                      options.LogoData, isSvg);
            row.RelativeItem()
                .AlignMiddle()
                .Column(column =>
                {
                    column.Item()
                        .Text(localizer["CashBook"] + " " + options.CashRegisterName)
                        .FontSize(20)
                        .Bold()
                        .AlignCenter();
                    column.Item()
                        .Text($"{options.Begin.ToString("d", CultureInfo.CurrentUICulture)} - " +
                              $"{options.End.ToString("d", CultureInfo.CurrentUICulture)}")
                        .AlignCenter();
                    column.Item().Height(5);

                });
            if (options.LogoData != null) RenderLogo(row.ConstantItem(100).AlignLeft().Height(70), 
                                                      options.LogoData, isSvg);
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
        var styleMatch = Regex.Match(svg, @"<style[^>]*>\s*(?:<!\[CDATA\[)?(.*?)(?:\]\]>)?\s*</style>", 
                                      RegexOptions.Singleline);
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

    private void ComposeContent(IContainer container, List<TransactionModel> ordered, PdfRenderOptions request)
    {
        var culture = CultureInfo.CurrentUICulture;

        container.PaddingTop(5, Unit.Millimetre).Column(col =>
        {
            ComposeTransactionTable(col, ordered, culture);
            ComposeSummaryTotals(col, ordered, culture);
            ComposeSignatureLines(col, request, culture);
        });
    }

    private void ComposeTransactionTable(ColumnDescriptor col, List<TransactionModel> ordered, CultureInfo culture)
    {
        var headerStyle = TextStyle.Default.SemiBold();

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(DateColumnWidth);
                columns.ConstantColumn(DocumentNumberColumnWidth);
                columns.RelativeColumn();
                columns.ConstantColumn(SumColumnWidth);
                columns.ConstantColumn(AccountColumnWidth);
            });

            table.Header(header =>
            {
                header.Cell().Element(CellHeaderStyle).Text($"{localizer["Date"]}")
                    .Style(headerStyle).AlignCenter();
                header.Cell().Element(CellHeaderStyle).Text($"{localizer["DocumentNumberShort"]}")
                    .Style(headerStyle).AlignCenter();
                header.Cell().Element(CellHeaderStyle).Text($"{localizer["Description"]}")
                    .Style(headerStyle);
                header.Cell().Element(CellHeaderStyle).Text($"{localizer["Sum"]}")
                    .Style(headerStyle).AlignCenter();
                header.Cell().Element(CellHeaderStyle).Text($"{localizer["Account"]}")
                    .Style(headerStyle).AlignCenter();

                IContainer CellHeaderStyle(IContainer containerHeader)
                    => DefaultCellStyle(containerHeader, HeaderBackgroundColor);
            });

            var index = 0;
            foreach (var t in ordered)
            {
                var background = GetRowBackgroundColor(index);

                table.Cell().Element(c => DefaultCellStyle(c, background)).Text(t.Date.ToString())
                    .AlignCenter();
                table.Cell().Element(c => DefaultCellStyle(c, background)).Text($"B{t.Documentnumber}")
                    .AlignCenter();
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
    }

    private void ComposeSummaryTotals(ColumnDescriptor col, List<TransactionModel> ordered, CultureInfo culture)
    {
        var totalIncome = ordered.Where(x => x.AccountMovement > 0)
                                        .Sum(x => x.AccountMovement);
        var totalExpense = ordered.Where(x => x.AccountMovement < 0)
                                         .Sum(x => x.AccountMovement);
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
    }

    private void ComposeSignatureLines(ColumnDescriptor col, PdfRenderOptions request, CultureInfo culture)
    {
        col.Item().Row(row =>
        {
            row.ConstantItem(100).AlignMiddle().Text($"{localizer["AccountBalance"]} " +
                                                         $"{request.Begin.ToString("d", culture)}:");
            row.ConstantItem(100).AlignBottom().LineHorizontal(1)
                .LineColor(Colors.Grey.Lighten1);
        });

        col.Item().PaddingTop(10).Row(row =>
        {
            row.ConstantItem(100).AlignMiddle().Text($"{localizer["AccountBalance"]} " +
                                                         $"{request.End.ToString("d", culture)}:");
            row.ConstantItem(100).AlignBottom().LineHorizontal(1)
                .LineColor(Colors.Grey.Lighten1);
        });

        col.Item().PaddingTop(10).Row(row =>
        {
            row.ConstantItem(100).AlignMiddle().Text($"{localizer["ClubTreasurer"]}:");
            row.ConstantItem(100).AlignBottom().Column(c =>
            {
                c.Item().Text(request.TreasurerName ?? string.Empty).AlignCenter();
                c.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
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