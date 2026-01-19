using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ExcelDataReader;
using Microsoft.Extensions.Localization;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.Transaction;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Import
{
    public class ImportBookingJournalService(
        CashDataContext context,
        ITransactionService transactionService,
        ILogger<ImportBookingJournalService> logger,
        IStringLocalizer<Translation> localizer)
        : IImportBookingJournalService
    {

        public async Task<bool> ImportTransactions(Stream? fileStream)
        {
            if (fileStream == null)
            {
                logger.LogError("The data-stream during import of bookingjournal is null.");
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
                var dataTable = result.Tables[localizer["Bookings"].Value];

                if (dataTable?.Rows == null)
                {
                    logger.LogWarning("No rows found in sheet: '{@name}'!", localizer["Bookings"]);
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
                    logger.LogError("Cash register not found: {@CashRegister}.", cashRegister);
                    return false;
                }

                var importCounter = 0;
                foreach (DataRow row in dataTable.Rows)
                {
                    try
                    {
                        if (!DateTime.TryParse(row.ItemArray.ElementAtOrDefault(0)?.ToString(), out var datum))
                        {
                            logger.LogError(
                                "Import failed: Date column missing or invalid. RowIndex={RowIndex}, RowData={@RowData}",
                                row.Table.Rows.IndexOf(row),
                                row.ItemArray);

                            return false;
                        }
                        var date = DateOnly.FromDateTime(datum);

                        var documentRaw = row.ItemArray[1]?.ToString()?.TrimStart('B');
                        if (!int.TryParse(documentRaw, out var documentNumber))
                        {
                            logger.LogWarning("Invalid document number in row {@RowData} at index {RowIndex}", 
                                row.ItemArray, row.Table.Rows.IndexOf(row));
                            continue;
                        }

                        if (existingDocumentNumbers.Contains(documentNumber))
                        {
                            logger.LogInformation("Skipping duplicate document number: {DocumentNumber}", documentNumber);
                            continue;
                        }

                        var description = row.ItemArray[2]?.ToString();

                        var sumStr = row.ItemArray[3]?.ToString()?.Trim();
                        if (string.IsNullOrEmpty(sumStr))
                            sumStr = row.ItemArray[4]?.ToString()?.Trim();

                        if (!decimal.TryParse(sumStr, out var sumValue))
                        {
                            logger.LogWarning("Missing or invalid sum value in row {@RowData}", row.ItemArray);
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
                            logger.LogWarning("Missing cost center info in row: {@RowData}", row.ItemArray);
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
                        importCounter++;

                        var addResult = await transactionService.AddTransactionAsync(transaction);
                        if (addResult.Status is OperationResultStatus.Failed)
                        {
                            logger.LogError("Failed to add transaction for document number: {DocumentNumber}", documentNumber);
                            return false;
                        }
                    }
                    catch (Exception rowEx)
                    {
                        logger.LogError(rowEx, "Error processing row: {@RowData}", row.ItemArray);
                        return false;
                    }
                }

                await dbTransaction.CommitAsync();
                if (importCounter > 0)
                {
                    logger.LogInformation("Booking journal was successfully imported.");
                    logger.LogInformation("Number of imported transactions: {ImportCounter}", importCounter);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,"An error occurred during import");
                return false;
            }
        }
    }
}
