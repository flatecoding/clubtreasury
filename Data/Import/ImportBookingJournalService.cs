using System.Data;
using Microsoft.EntityFrameworkCore;
using ExcelDataReader;
using TTCCashRegister.Data.BasicUnit;
using TTCCashRegister.Data.CostUnit;
using TTCCashRegister.Data.Transaction;

namespace TTCCashRegister.Data.Import
{
   public class ImportBookingJournalService(CashDataContext context, TransactionService transactionService, 
       ILogger<ImportBookingJournalService> logger)
   {
    public async Task<bool> ImportTransactions(Stream? fileStream)
    {
        if (fileStream == null)
        {
            Console.WriteLine("The data-stream is null.");
            logger.LogError(0, "The data-stream is null.");
            return false;
        }

        await using var dbtransaction = await context.Database.BeginTransactionAsync();

        try
        {
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var reader = ExcelReaderFactory.CreateReader(memoryStream);
            var result = reader.AsDataSet();
            var dataTable = result.Tables["Buchungen"];

            var costUnits = await context.CostUnits
                .Include(cu => cu.BasicUnitDetails)
                .ToListAsync();

            var basicUnits = await context.BasicUnits
                .ToListAsync();

            var cashregister = await context.CashRegisters.FirstOrDefaultAsync();

            if (dataTable?.Rows != null)
                foreach (DataRow row in dataTable.Rows)
                {
                    var datum = Convert.ToDateTime(row.ItemArray[0]);
                    var date = DateOnly.FromDateTime(datum); // Umwandlung in DateOnly
                    var documentNumber = row.ItemArray[1]?.ToString()?.TrimStart('B');
                    var description = row.ItemArray[2]?.ToString();
                    var sumValue = row.ItemArray[3] != DBNull.Value
                        ? Convert.ToDecimal(row.ItemArray[3])
                        : Convert.ToDecimal(row.ItemArray[4]);
                    var accountMovement = row.ItemArray[5]?.ToString();
                    var costUnitBasicUnit = row.ItemArray[6]?.ToString();
                    var parts = costUnitBasicUnit?.Split('/');

                    string costUnitName;
                    string basicUnitName;
                    switch (parts?.Length)
                    {
                        case 1:
                        {
                            costUnitName = parts[0].Trim();
                            basicUnitName = "Undefined";
                        }
                            break;
                        case >= 2:
                        {
                            costUnitName = parts[0].Trim();
                            basicUnitName = parts[1].Trim();
                        }
                            break;
                        default: continue;
                    }

                    var costUnit = costUnits.FirstOrDefault(cu => cu.CostUnitName == costUnitName);

                    if (costUnit == null)
                    {
                        costUnit = new CostUnitModel { CostUnitName = costUnitName };
                        costUnits.Add(costUnit);
                        context.CostUnits.Add(costUnit);
                    }

                    var basicUnit =
                        basicUnits.FirstOrDefault(bu => bu.Name == basicUnitName && bu.CostUnitId == costUnit.Id);

                    if (basicUnit == null)
                    {
                        basicUnit = new BasicUnitModel { Name = basicUnitName, CostUnit = costUnit };
                        costUnit.BasicUnitDetails.Add(basicUnit);
                        basicUnits.Add(basicUnit);
                        context.BasicUnits.Add(basicUnit);
                    }
                    
                    var transaction = new TransactionModel
                    {
                        CashRegisterId = cashregister.ID,
                        Date = date, // Verwendung von DateOnly
                        Documentnumber = Convert.ToInt32(documentNumber),
                        Description = description,
                        Sum = sumValue,
                        AccountMovement = Convert.ToDecimal(accountMovement),
                        CostUnitId = costUnit.Id,
                        CostUnit = costUnit,
                        BasicUnitId = basicUnit.Id,
                        BasicUnit = basicUnit,
                    };
                    
                    if (!await transactionService.AddTransaction(transaction))
                    {
                        logger.LogError(0, $"An error occured during import of booking journal'{date}'");
                        return false;
                    }
                }

            await dbtransaction.CommitAsync();
            logger.Log(LogLevel.Information, "Booking journal was succesfully imported.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAn error occured during import of booking journal: {ex.Message}");
            logger.LogError($"FAn error occured during import of booking journal: {ex.Message}");
            return false;
        }
    }
}
}
