using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;

namespace TTCCashRegister.Data.Import
{
    public class ImportCostCenterService(CashDataContext context, ILogger<ImportCostCenterService> logger,
    IStringLocalizer<Translation> localizer) : IImportCostCenterService
    {
        public async Task<bool> ImportCostCentersAndPositions(Stream fileStream)
        {
            if (fileStream == null)
            {
                logger.LogError("Import cost center file stream is null");
                return false;
            }

            try
            {
                logger.LogDebug("Start import of cost centers");
                using var reader = new StreamReader(fileStream);
                var lines = new List<string>();

                while (await reader.ReadLineAsync() is { } currentLine)
                {
                    if (!string.IsNullOrWhiteSpace(currentLine))
                    {
                        lines.Add(currentLine);
                    }
                }

                var costCenters = await context.CostCenters.ToListAsync();
                var categories = await context.Categories.ToListAsync();
                var allocations = await context.Allocations.ToListAsync();

                foreach (var line in lines)
                {
                    var parts = line.Split('/');
                    string costCenterName;
                    string categoryName;

                    switch (parts.Length)
                    {
                        case 1:
                            costCenterName = parts[0].Trim();
                            categoryName = $"{localizer["Undefined"]}";
                            break;
                        case >= 2:
                            costCenterName = parts[0].Trim();
                            categoryName = parts[1].Trim();
                            break;
                        default:
                            continue;
                    }

                    var costCenter = costCenters.FirstOrDefault(c => c.CostUnitName == costCenterName);
                    if (costCenter == null)
                    {
                        costCenter = new CostCenterModel { CostUnitName = costCenterName };
                        costCenters.Add(costCenter);
                        context.CostCenters.Add(costCenter);
                        logger.LogInformation("Add cost center: '{@CostCenter}' during import ", costCenter);
                    }

                    var category = categories.FirstOrDefault(c => c.Name == categoryName);
                    if (category == null)
                    {
                        category = new CategoryModel { Name = categoryName };
                        categories.Add(category);
                        context.Categories.Add(category);
                        logger.LogInformation("Add category: '{@Category}' during import ", category);
                    }

                    var allocationExists = allocations.Any(a =>
                        a.CostCenterId == costCenter.Id &&
                        a.CategoryId == category.Id &&
                        a.ItemDetailId == null);

                    if (!allocationExists)
                    {
                        var allocation = new AllocationModel
                        {
                            CostCenter = costCenter,
                            Category = category,
                            ItemDetailId = null
                        };
                        context.Allocations.Add(allocation);
                        allocations.Add(allocation);
                        logger.LogInformation("Add allocation: '{@Allocation}' during import ", allocation);
                    }
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Import of const centers completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Import of const centers failed");
                return false;
            }
        }
    }
}
