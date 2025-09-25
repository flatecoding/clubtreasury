using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ExcelDataReader;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.Transaction;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;

namespace TTCCashRegister.Data.Import
{
    public class ImportBookingJournalService
    {
        private readonly CashDataContext context;
        private readonly TransactionService transactionService;
        private readonly ILogger<ImportBookingJournalService> logger;

        public ImportBookingJournalService(CashDataContext context, TransactionService transactionService, ILogger<ImportBookingJournalService> logger)
        {
            this.context = context;
            this.transactionService = transactionService;
            this.logger = logger;
        }

        public async Task<bool> ImportTransactions(Stream? fileStream)
        {
            if (fileStream == null)
            {
                logger.LogError("The data-stream is null.");
                return false;
            }

            await using var dbTransaction = await context.Database.BeginTransactionAsync();
            try
            {
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using var reader = ExcelReaderFactory.CreateReader(memoryStream);
                var result = reader.AsDataSet();
                var dataTable = result.Tables["Buchungen"];

                if (dataTable?.Rows == null)
                {
                    logger.LogWarning("No rows found in 'Buchungen' sheet.");
                    return false;
                }

                var existingDocumentNumbers = new HashSet<int>(
                    await context.Transactions.Select(t => t.Documentnumber).ToListAsync()
                );

                var costCenters = await context.CostCenters.ToListAsync();
                var categories = await context.Categories.ToListAsync();
                var allocations = await context.Allocations.ToListAsync();
                var cashRegister = await context.CashRegisters.FirstOrDefaultAsync();

                if (cashRegister == null)
                {
                    logger.LogError("No cash register found in database.");
                    return false;
                }

                foreach (DataRow row in dataTable.Rows)
                {
                    try
                    {
                        if (!DateTime.TryParse(row.ItemArray[0]?.ToString(), out var datum))
                        {
                            logger.LogWarning("Invalid date format in row: {0}", string.Join(", ", row.ItemArray));
                            continue;
                        }
                        var date = DateOnly.FromDateTime(datum);

                        var documentRaw = row.ItemArray[1]?.ToString()?.TrimStart('B');
                        if (!int.TryParse(documentRaw, out var documentNumber))
                        {
                            logger.LogWarning("Invalid document number in row: {0}", string.Join(", ", row.ItemArray));
                            continue;
                        }

                        if (existingDocumentNumbers.Contains(documentNumber))
                        {
                            logger.LogInformation("Skipping duplicate document number: {0}", documentNumber);
                            continue;
                        }

                        var description = row.ItemArray[2]?.ToString();

                        var sumStr = row.ItemArray[3]?.ToString()?.Trim();
                        if (string.IsNullOrEmpty(sumStr))
                            sumStr = row.ItemArray[4]?.ToString()?.Trim();

                        if (!decimal.TryParse(sumStr, out var sumValue))
                        {
                            logger.LogWarning("Missing or invalid sum value in row: {0}", string.Join(", ", row.ItemArray));
                            continue;
                        }

                        var accountMovement = 0m;
                        if (row.ItemArray[5] != DBNull.Value && decimal.TryParse(row.ItemArray[5]?.ToString(), out var accMove))
                        {
                            accountMovement = accMove;
                        }

                        var costCenterCategory = row.ItemArray[6]?.ToString();
                        var parts = costCenterCategory?.Split('/');
                        if (parts == null || parts.Length == 0)
                        {
                            logger.LogWarning("Missing cost center info in row: {0}", string.Join(", ", row.ItemArray));
                            continue;
                        }

                        var costCenterName = parts[0].Trim();
                        var categoryName = parts.Length >= 2 ? parts[1].Trim() : "Undefined";

                        var costCenter = costCenters.FirstOrDefault(c => c.CostUnitName == costCenterName);
                        if (costCenter == null)
                        {
                            costCenter = new CostCenterModel { CostUnitName = costCenterName };
                            costCenters.Add(costCenter);
                            context.CostCenters.Add(costCenter);
                        }

                        var category = categories.FirstOrDefault(c => c.Name == categoryName);
                        if (category == null)
                        {
                            category = new CategoryModel { Name = categoryName };
                            categories.Add(category);
                            context.Categories.Add(category);
                        }

                        var allocation = allocations.FirstOrDefault(a =>
                            a.CostCenterId == costCenter.Id &&
                            a.CategoryId == category.Id &&
                            a.ItemDetailId == null);

                        if (allocation == null)
                        {
                            allocation = new AllocationModel
                            {
                                CostCenter = costCenter,
                                Category = category,
                                ItemDetailId = null
                            };
                            allocations.Add(allocation);
                            context.Allocations.Add(allocation);
                        }

                        var transaction = new TransactionModel
                        {
                            CashRegisterId = cashRegister.Id,
                            Date = date,
                            Documentnumber = documentNumber,
                            Description = description,
                            Sum = sumValue,
                            AccountMovement = accountMovement,
                            Allocation = allocation
                        };

                        if (!await transactionService.AddTransactionAsync(transaction))
                        {
                            logger.LogError("Failed to add transaction for document number {0}", documentNumber);
                            return false;
                        }
                    }
                    catch (Exception rowEx)
                    {
                        logger.LogError("Error processing row: {0} — {1}", string.Join(", ", row.ItemArray), rowEx.Message);
                        return false;
                    }
                }

                await dbTransaction.CommitAsync();
                logger.LogInformation("Booking journal was successfully imported.");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("An error occurred during import: {0}", ex.Message);
                return false;
            }
        }
    }
}
