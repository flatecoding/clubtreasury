using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;

namespace TTCCashRegister.Data.Import
{
    public class ImportCostCenterService(CashDataContext context)
    {
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

                var costCenters = await context.CostCenters
                    .Include(cu => cu.Categories)
                    .ToListAsync();

                var categories = await context.Categories
                    .ToListAsync();

                foreach (var line in lines)
                {
                    var parts = line.Split('/');
                    string costCenterName;
                    string positionName;
                    switch (parts.Length)
                    {
                        case 1:
                        {
                            costCenterName = parts[0].Trim();
                            positionName = "Undefined"; 
                        }
                            break;
                        case >= 2:
                        {
                            costCenterName = parts[0].Trim();
                            positionName = parts[1].Trim();
                        }
                            break;
                        default: continue;
                    }

                    var costCenter = costCenters.FirstOrDefault(cu => cu.CostUnitName == costCenterName);

                    if (costCenter == null)
                    {
                        costCenter = new CostCenterModel { CostUnitName = costCenterName };
                        costCenters.Add(costCenter);
                        context.CostCenters.Add(costCenter);
                    }

                    var category = categories.FirstOrDefault(bu => bu.Name == positionName && bu.CostCenterId == costCenter.Id);

                    if (category == null)
                    {
                        category = new CategoryModel { Name = positionName, CostCenter = costCenter };
                        costCenter.Categories.Add(category);
                        categories.Add(category);
                        context.Categories.Add(category);
                    }
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Lesen des Datei-Streams: {ex.Message}");
                return false;
            }
        }
    }//ImportCostunitService
    
}//namespace