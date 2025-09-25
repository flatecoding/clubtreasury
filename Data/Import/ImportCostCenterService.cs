using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;

namespace TTCCashRegister.Data.Import
{
    public class ImportCostCenterService
    {
        private readonly CashDataContext _context;

        public ImportCostCenterService(CashDataContext context)
        {
            _context = context;
        }

        public async Task<bool> ImportCostCentersAndPositions(Stream fileStream)
        {
            if (fileStream == null)
            {
                Console.WriteLine("Der Datei-Stream ist null.");
                return false;
            }

            try
            {
                using var reader = new StreamReader(fileStream);
                var lines = new List<string>();

                while (await reader.ReadLineAsync() is { } currentline)
                {
                    if (!string.IsNullOrWhiteSpace(currentline))
                    {
                        lines.Add(currentline);
                    }
                }

                var costCenters = await _context.CostCenters.ToListAsync();
                var categories = await _context.Categories.ToListAsync();
                var allocations = await _context.Allocations.ToListAsync();

                foreach (var line in lines)
                {
                    var parts = line.Split('/');
                    string costCenterName;
                    string categoryName;

                    switch (parts.Length)
                    {
                        case 1:
                            costCenterName = parts[0].Trim();
                            categoryName = "Undefined";
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
                        _context.CostCenters.Add(costCenter);
                    }

                    var category = categories.FirstOrDefault(c => c.Name == categoryName);
                    if (category == null)
                    {
                        category = new CategoryModel { Name = categoryName };
                        categories.Add(category);
                        _context.Categories.Add(category);
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
                        _context.Allocations.Add(allocation);
                        allocations.Add(allocation);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Lesen des Datei-Streams: {ex.Message}");
                return false;
            }
        }
    }
}
