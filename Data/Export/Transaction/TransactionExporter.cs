using System.Text;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Data.Export.Transaction;

public class TransactionExporter(
    ITransactionService transactionService,
    IPdfTransactionRenderer pdfRenderer,
    ICashRegisterLogoService cashRegisterLogoService,
    IResultFactory operationResultFactory,
    IStringLocalizer<Translation> localizer,
    ILogger<TransactionExporter> logger,
    IExportPathProvider exportPathProvider
) : ITransactionExporter
{
    private const string ExceptionKey = "Exception";

    private string CsvHeader =>
        $"{localizer["DocumentNumberShort"]};{localizer["Description"]};{localizer["Sum"]};{localizer["Account"]}";

    private readonly string _exportPath = exportPathProvider.ExportPath;

    private string GetSafeFilePath(string filename)
    {
        var sanitized = Path.GetFileName(filename);
        if (string.IsNullOrWhiteSpace(sanitized) || sanitized.Contains(".."))
            throw new ArgumentException("Invalid filename");
        return Path.Combine(_exportPath, sanitized);
    }

    public async Task<Result> ExportToCsvAsync(ExportOptions options, CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Beginning export transactions to csv");

            var transactions =
                await transactionService.GetTransactionsForExportAsync(options.Begin, options.End, options.CashRegisterId, ct);

            var csv = new StringBuilder();
            foreach (var transaction in transactions.OrderBy(t => t.Documentnumber))
            {
                csv.AppendLine(
                    $"{transaction.Documentnumber};{transaction.Description};{transaction.Sum};{transaction.AccountMovement}");
            }

            if (csv.Length == 0)
            {
                logger.LogError("CSV file for transactions could not be created");
                return operationResultFactory.ExportFailed($"{localizer["NoData"]}");
            }

            var filePath = GetSafeFilePath(options.Filename);
            await using var sw = new StreamWriter(filePath);
            await sw.WriteLineAsync(CsvHeader);
            await sw.WriteAsync(csv);

            logger.LogInformation("Export transactions to csv completed");
            return operationResultFactory.ExportSuccessful(options.Filename);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while exporting transactions to csv");
            return operationResultFactory.ExportFailed($"{localizer[ExceptionKey]}");
        }
    }

    public async Task<Result> ExportToPdfAsync(PdfExportOptions options, CancellationToken ct = default)
    {
        try
        {
            var transactions =
                await transactionService.GetTransactionsForExportAsync(options.Begin, options.End, options.CashRegisterId, ct);

            var logo = await cashRegisterLogoService.GetLogoAsync(options.CashRegisterId, ct);
            var filePath = GetSafeFilePath(options.Filename);

            var renderRequest = new PdfRenderOptions(
                options.Begin, options.End, filePath,
                options.CashRegisterName, options.TreasurerName,
                logo?.Data, logo?.ContentType);

            await pdfRenderer.RenderTransactionPdfExportAsync(transactions, renderRequest, ct);

            return operationResultFactory.ExportSuccessful(options.Filename);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("PDF export canceled → {File}", options.Filename);
            return operationResultFactory.Canceled();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PDF export failed");
            return operationResultFactory.ExportFailed($"{localizer[ExceptionKey]}");
        }
    }
}