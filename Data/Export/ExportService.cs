using System.Text;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.Export.Budget;
using ClubTreasury.Data.Export.Transaction;
using ClubTreasury.Data.Mapper;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Data.Export;

public class ExportService(
    ITransactionService transactionService,
    ILogger<ExportService> logger,
    IBudgetMapper budgetMapper,
    ICsvBudgetWriter csvWriter,
    IExcelBudgetWriter excelWriter,
    IPdfTransactionRenderer transactionPdfRenderer,
    IOperationResultFactory operationResultFactory,
    IStringLocalizer<Resources.Translation> localizer,
    IExportPathProvider exportPathProvider,
    ICashRegisterService cashRegisterService
) : IExportService
{
    private const string CsvHeader = 
        "Belegnr.;Beschreibung;Rechnungsbetrag;Kontobewegung";

    private readonly string _exportPath = exportPathProvider.ExportPath;

    private string GetSafeFilePath(string filename)
    {
        var sanitized = Path.GetFileName(filename);
        if (string.IsNullOrWhiteSpace(sanitized) || sanitized.Contains(".."))
            throw new ArgumentException("Invalid filename");
        return Path.Combine(_exportPath, sanitized);
    }

    public async Task<IOperationResult> ExportTransactionsToCsv(
        DateTime begin, DateTime end, string filename, int cashRegisterId)
    {
        try
        {
            logger.LogInformation("Beginning export transactions to csv");

            var transactions =
                await transactionService.GetTransactionsForExport(begin, end, cashRegisterId);

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

            var filePath = GetSafeFilePath(filename);
            await using var sw = new StreamWriter(filePath);
            await sw.WriteLineAsync(CsvHeader);
            await sw.WriteAsync(csv);

            logger.LogInformation("Export transactions to csv completed");
            return operationResultFactory.ExportSuccessful(filename);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while exporting transactions to csv");
            return operationResultFactory.ExportFailed($"{localizer["Exception"]}");
        }
    }

    public async Task<IOperationResult> ExportTransactionsToPdf(
        DateTime begin, DateTime end, string filename, int cashRegisterId, string cashRegisterName, CancellationToken cancellationToken)
    {
        try
        {
            var transactions =
                await transactionService.GetTransactionsForExport(begin, end, cashRegisterId);

            var logo = await cashRegisterService.GetLogoAsync(cashRegisterId);
            var filePath = GetSafeFilePath(filename);

            await transactionPdfRenderer.RenderTransactionPdfExportAsync(
                transactions, begin, end, filePath, cashRegisterName, logo?.Data, logo?.ContentType, cancellationToken);

            return operationResultFactory.ExportSuccessful(filename);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("PDF export canceled → {File}", filename);
            return operationResultFactory.Canceled();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PDF export failed");
            return operationResultFactory.ExportFailed($"{localizer["Exception"]}");
        }
    }

    public async Task<IOperationResult> ExportBudgetToCsv(
        DateTime begin, DateTime end, string filename, int cashRegisterId)
    {
        try
        {
            var transactions =
                await transactionService.GetTransactionsForBudgetExport(begin, end, cashRegisterId);

            var flat = budgetMapper.BuildFlatEntries(transactions);
            var grouped = budgetMapper.BuildBudgetHierarchy(flat);

            var filePath = GetSafeFilePath(filename);
            await csvWriter.WriteAsync(filePath, grouped);

            return operationResultFactory.ExportSuccessful(filename);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating budget csv");
            return operationResultFactory.ExportFailed(ex.Message);
        }
    }

    public async Task<IOperationResult> ExportBudgetToExcel(
        DateTime begin, DateTime end, string filename, int cashRegisterId)
    {
        try
        {
            var transactions =
                await transactionService.GetTransactionsForBudgetExport(begin, end, cashRegisterId);

            var flat = budgetMapper.BuildFlatEntries(transactions);
            var grouped = budgetMapper.BuildBudgetHierarchy(flat);

            var filePath = GetSafeFilePath(filename);
            await excelWriter.WriteAsync(filePath, grouped, begin, end);

            return operationResultFactory.ExportSuccessful(filename);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Export budget to Excel failed");
            return operationResultFactory.ExportFailed(ex.Message);
        }
    }

    public async Task<byte[]> ExportBudgetToExcelBytes(
        DateTime begin, DateTime end, int cashRegisterId)
    {
        var filename = $"Budget_{begin:yyyyMMdd}_{end:yyyyMMdd}.xlsx";
        var result = await ExportBudgetToExcel(begin, end, filename, cashRegisterId);

        if (result.Status != OperationResultStatus.Success)
            return Array.Empty<byte>();

        var filePath = GetSafeFilePath(filename);
        return await File.ReadAllBytesAsync(filePath);
    }
}