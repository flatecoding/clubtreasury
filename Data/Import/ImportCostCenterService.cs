using Microsoft.Extensions.Localization;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Import;

public class ImportCostCenterService(
    IAllocationService allocationService, 
    ILogger<ImportCostCenterService> logger,
    IStringLocalizer<Resources.Translation> localizer,
    IOperationResultFactory operationResultFactory) 
    : IImportCostCenterService
{
    public async Task<IOperationResult> ImportCostCentersAndPositions(
        Stream? fileStream, 
        string fileName)
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
                await allocationService.GetOrCreateAllocationAsync(
                    costCenterName, 
                    categoryName);
                importCount++;
            }

            logger.LogInformation(
                "Import of cost centers completed. Count: {Count}", 
                importCount);
            
            return operationResultFactory.ImportSuccessful(fileName);
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