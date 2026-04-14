using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Import;

public class ImportCostCenterService(
    IAllocationService allocationService,
    ILogger<ImportCostCenterService> logger,
    IStringLocalizer<Translation> localizer,
    IResultFactory operationResultFactory)
    : IImportCostCenterService
{
    public async Task<Result> ImportCostCentersAndPositionsAsync(
        Stream? fileStream,
        string fileName,
        CancellationToken ct = default)
    {
        if (fileStream == null)
        {
            logger.LogError("Import cost center file stream is null");
            return operationResultFactory.ImportFailed(localizer["FileStreamError"]);
        }

        try
        {
            logger.LogDebug("Start import of cost centers");

            var lines = await ParseFile(fileStream);

            var importCount = 0;
            foreach (var (costCenterName, categoryName) in lines)
            {
                ct.ThrowIfCancellationRequested();

                await allocationService.GetOrCreateAllocationAsync(
                    costCenterName,
                    categoryName,
                    ct: ct);
                importCount++;
            }

            logger.LogInformation(
                "Import of cost centers completed. Count: {Count}",
                importCount);

            return operationResultFactory.ImportSuccessful(fileName);
        }
        catch (OperationCanceledException operationCanceledException)
        {
            logger.LogWarning(operationCanceledException, "Import of cost centers was canceled");
            return operationResultFactory.Canceled();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Import of cost centers failed");
            return operationResultFactory.ImportFailed(localizer["Exception"]);
        }
    }

    private async Task<List<(string CostCenter, string Category)>> ParseFile(Stream fileStream)
    {
        var result = new List<(string, string)>();

        using var reader = new StreamReader(fileStream);
        while (await reader.ReadLineAsync() is { } currentLine)
        {
            if (string.IsNullOrWhiteSpace(currentLine))
                continue;

            var parts = currentLine.Split('/');

            var costCenterName = parts[0].Trim();
            var categoryName = parts.Length >= 2
                ? parts[1].Trim()
                : localizer["Undefined"].Value;

            result.Add((costCenterName, categoryName));
        }

        return result;
    }
}