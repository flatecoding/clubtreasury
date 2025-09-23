using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.BasicUnit;
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
                    .Include(cu => cu.BasicUnitDetails)
                    .ToListAsync();

                var basicUnits = await context.BasicUnits
                    .ToListAsync();

                foreach (var line in lines)
                {
                    var parts = line.Split('/');
                    string costUnitName;
                    string positionName;
                    switch (parts.Length)
                    {
                        case 1:
                        {
                            costUnitName = parts[0].Trim();
                            positionName = "Undefined"; 
                        }
                            break;
                        case >= 2:
                        {
                            costUnitName = parts[0].Trim();
                            positionName = parts[1].Trim();
                        }
                            break;
                        default: continue;
                    }

                    var costCenter = costCenters.FirstOrDefault(cu => cu.CostUnitName == costUnitName);

                    if (costCenter == null)
                    {
                        costCenter = new CostCenterModel { CostUnitName = costUnitName };
                        costCenters.Add(costCenter);
                        context.CostCenters.Add(costCenter);
                    }

                    var basicUnit = basicUnits.FirstOrDefault(bu => bu.Name == positionName && bu.CostCenterId == costCenter.Id);

                    if (basicUnit == null)
                    {
                        basicUnit = new BasicUnitModel { Name = positionName, CostCenter = costCenter };
                        costCenter.BasicUnitDetails.Add(basicUnit);
                        basicUnits.Add(basicUnit);
                        context.BasicUnits.Add(basicUnit);
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