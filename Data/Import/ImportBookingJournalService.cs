using System.Data;
using System.Text;
using ExcelDataReader;
using Microsoft.Extensions.Localization;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.CashRegister;
using TTCCashRegister.Data.OperationResult;
using TTCCashRegister.Data.Transaction;

namespace TTCCashRegister.Data.Import;

public class ImportBookingJournalService(
    ITransactionService transactionService,
    IAllocationService allocationService,     
    ICashRegisterService cashRegisterService,
    ILogger<ImportBookingJournalService> logger,
    IStringLocalizer<Translation> localizer,
    IOperationResultFactory operationResultFactory)
    : IImportBookingJournalService
{
    public async Task<IOperationResult> ImportTransactions(Stream? fileStream, string fileName)
    {
        if (fileStream == null)
        {
            logger.LogError("The data-stream during import of booking journal is null.");
            return operationResultFactory.ImportFailed(localizer["FileStreamError"]);
        }

        try
        {
            var parsedRows = await ParseExcelFile(fileStream);
            if (parsedRows == null || parsedRows.Count == 0)
            {
                return operationResultFactory.ImportFailed(localizer["NoData"]);
            }
            
            var validationResult = await ValidateRows(parsedRows);
            if (!validationResult.IsValid)
            {
                return validationResult.ErrorResult!;
            }
            
            var cashRegister = await cashRegisterService.GetFirstCashRegisterAsync();
            if (cashRegister == null)
            {
                logger.LogError("Cash register not found.");
                return operationResultFactory.NotFound(localizer["CashRegister"], 0);
            }
            
            var importCounter = 0;
            var existingDocNumbers = await transactionService.GetAllDocumentNumbersAsync();
            
            foreach (var row in parsedRows)
            {
                try
                {
                    if (existingDocNumbers.Contains(row.DocumentNumber))
                    {
                        logger.LogInformation(
                            "Skipping duplicate document number: {DocumentNumber}", 
                            row.DocumentNumber);
                        continue;
                    }
                    
                    var allocation = await allocationService.GetOrCreateAllocationAsync(
                        row.CostCenterName,
                        row.CategoryName);
                    
                    var transaction = new TransactionModel
                    {
                        CashRegisterId = cashRegister.Id,
                        Date = row.Date,
                        Documentnumber = row.DocumentNumber,
                        Description = row.Description,
                        Sum = row.Sum,
                        AccountMovement = row.AccountMovement,
                        AllocationId = allocation.Id
                    };

                    var addResult = await transactionService.AddTransactionAsync(transaction);
                    if (addResult.Status == OperationResultStatus.Failed)
                    {
                        logger.LogError(
                            "Failed to add transaction for document number: {DocumentNumber}", 
                            row.DocumentNumber);
                        return operationResultFactory.ImportFailed(
                            $"{localizer["DocumentNumberError"]}: '{row.DocumentNumber}'");
                    }
                    existingDocNumbers.Add(row.DocumentNumber);
                    importCounter++;
                }
                catch (Exception rowEx)
                {
                    logger.LogError(rowEx, "Error processing row: {@Row}", row);
                    //skip row, continue with next line
                }
            }

            logger.LogInformation(
                "Booking journal successfully imported. Transactions: {Count}", 
                importCounter);
            
            return operationResultFactory.ImportSuccessful(fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during import");
            return operationResultFactory.ImportFailed(localizer["Exception"]);
        }
    }
    
    private async Task<List<BookingJournalRowDto>?> ParseExcelFile(Stream fileStream)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var reader = ExcelReaderFactory.CreateReader(memoryStream);
        var result = reader.AsDataSet();
        var dataTable = result.Tables["Bookings"];

        if (dataTable?.Rows == null)
        {
            logger.LogWarning("No rows found in sheet: 'Bookings'!");
            return null;
        }

        var rows = new List<BookingJournalRowDto>();
        foreach (DataRow row in dataTable.Rows)
        {
            var parsedRow = ParseRow(row);
            if (parsedRow != null)
            {
                rows.Add(parsedRow);
            }
        }

        return rows;
    }

    private BookingJournalRowDto? ParseRow(DataRow row)
    {
        try
        {
            const int cDateCell = 0;
            if (!DateTime.TryParse(row.ItemArray.ElementAtOrDefault(cDateCell)?.ToString(), out var datum))
            {
                logger.LogWarning("Invalid date in row: {@RowData}", row.ItemArray);
                return null;
            }

            const int cDocumentCell = 1;
            var documentRaw = row.ItemArray[cDocumentCell]?.ToString()?.TrimStart('B');
            if (!int.TryParse(documentRaw, out var documentNumber))
            {
                logger.LogWarning("Invalid document number in row: {@RowData}", row.ItemArray);
                return null;
            }

            const int cDescriptionCell = 2;
            var description = row.ItemArray[cDescriptionCell]?.ToString();

            const int cSumIncomeCell = 3;
            const int cSumOutcomeCell = 4;
            
            var sumStr = row.ItemArray[cSumIncomeCell]?.ToString()?.Trim();
            if (string.IsNullOrEmpty(sumStr))
                sumStr = row.ItemArray[cSumOutcomeCell]?.ToString()?.Trim();

            if (!decimal.TryParse(sumStr, out var sumValue))
            {
                logger.LogWarning("Invalid sum in row: {@RowData}", row.ItemArray);
                return null;
            }

            var accountMovement = 0m;
            const int cAccountMovementCell = 5;
            if (row.ItemArray[cAccountMovementCell] != DBNull.Value && 
                decimal.TryParse(row.ItemArray[5]?.ToString(), out var accMove))
            {
                accountMovement = accMove;
            }

            const int cCostCenterCategoryCell = 6;
            var costCenterCategory = row.ItemArray[cCostCenterCategoryCell]?.ToString();
            var parts = costCenterCategory?.Split('/');
            if (parts == null || parts.Length == 0)
            {
                logger.LogWarning("Missing cost center info in row: {@RowData}", row.ItemArray);
                return null;
            }

            return new BookingJournalRowDto
            {
                Date = DateOnly.FromDateTime(datum),
                DocumentNumber = documentNumber,
                Description = description,
                Sum = sumValue,
                AccountMovement = accountMovement,
                CostCenterName = parts[0].Trim(),
                CategoryName = parts.Length >= 2 ? parts[1].Trim() : "Undefined"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing row: {@RowData}", row.ItemArray);
            return null;
        }
    }

    private Task<ValidationResult> ValidateRows(
        List<BookingJournalRowDto> rows)
    {
        if (rows == null || rows.Count == 0)
        {
            return Task.FromResult(new ValidationResult
            {
                IsValid = false,
                ErrorResult = operationResultFactory.ImportFailed(
                    localizer["NoData"])
            });
        }

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNumber = i + 1;

            if (row.Date == default)
                return Invalid(rowNumber, localizer["DateMissing"]);

            if (row.Date > DateOnly.FromDateTime(DateTime.Today))
                return Invalid(rowNumber, localizer["DateInFuture"]);

            if (row.DocumentNumber <= 0)
                return Invalid(rowNumber, localizer["InvalidDocumentNumber"]);

            if (row.Sum <= 0)
                return Invalid(rowNumber, localizer["InvalidSum"]);

            if (row.AccountMovement == 0)
                return Invalid(rowNumber, localizer["InvalidAccountMovement"]);

            if (Math.Abs(row.AccountMovement) != row.Sum)
                return Invalid(rowNumber, localizer["SumAccountMismatch"]);

            if (string.IsNullOrWhiteSpace(row.CostCenterName))
                return Invalid(rowNumber, localizer["CostCenterMissing"]);

            if (string.IsNullOrWhiteSpace(row.CategoryName))
                return Invalid(rowNumber, localizer["CategoryMissing"]);
        }

        var duplicate = rows
            .GroupBy(r => new
            {
                r.Date,
                r.DocumentNumber,
                r.Sum,
                r.AccountMovement
            })
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate != null)
        {
            return Task.FromResult(new ValidationResult
            {
                IsValid = false,
                ErrorResult = operationResultFactory.ImportFailed(
                    localizer["DuplicateRow"])
            });
        }

        return Task.FromResult(new ValidationResult
        {
            IsValid = true
        });
    }
    
    private Task<ValidationResult> Invalid(
        int rowNumber,
        string message)
    {
        return Task.FromResult(new ValidationResult
        {
            IsValid = false,
            ErrorResult = operationResultFactory.ImportFailed(
                string.Format(
                    localizer["RowErrorFormat"],
                    rowNumber,
                    message))
        });
    }

}

public record ValidationResult
{
    public bool IsValid { get; init; }
    public IOperationResult? ErrorResult { get; init; }
}

