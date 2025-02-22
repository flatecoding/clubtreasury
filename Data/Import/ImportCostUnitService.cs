using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.CostUnit;
using TTCCashRegister.Data.BasicUnit;

namespace TTCCashRegister.Data.Import
{
    public class ImportCostUnitService
    {
        private readonly CashDataContext _context;

        public ImportCostUnitService(CashDataContext context)
        {
            _context = context;
        }
        
        public async Task<bool> ImportCostUnitsAndPositions(Stream fileStream)
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

                var costUnits = await _context.CostUnits
                    .Include(cu => cu.BasicUnitDetails)
                    .ToListAsync();

                var basicUnits = await _context.BasicUnits
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

                    var costUnit = costUnits.FirstOrDefault(cu => cu.CostUnitName == costUnitName);

                    if (costUnit == null)
                    {
                        costUnit = new CostUnitModel { CostUnitName = costUnitName };
                        costUnits.Add(costUnit);
                        _context.CostUnits.Add(costUnit);
                    }

                    var basicUnit = basicUnits.FirstOrDefault(bu => bu.Name == positionName && bu.CostUnitId == costUnit.Id);

                    if (basicUnit == null)
                    {
                        basicUnit = new BasicUnitModel { Name = positionName, CostUnit = costUnit };
                        costUnit.BasicUnitDetails.Add(basicUnit);
                        basicUnits.Add(basicUnit);
                        _context.BasicUnits.Add(basicUnit);
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
    }//ImportCostunitService
    
}//namespace