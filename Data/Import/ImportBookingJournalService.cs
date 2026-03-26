using System.Data;
using ExcelDataReader;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Data.Import;

public class ImportBookingJournalService(
    ITransactionService transactionService,
    IAllocationService allocationService,
    ICashRegisterService cashRegisterService,
    ILogger<ImportBookingJournalService> logger,
    IStringLocalizer<Translation> localizer,
    IResultFactory operationResultFactory)
    : IImportBookingJournalService
{
    private const int DateCell = 0;
    private const int DocumentCell = 1;
    private const int DescriptionCell = 2;
    private const int SumIncomeCell = 3;
    private const int SumOutcomeCell = 4;
    private const int AccountMovementCell = 5;
    private const int CostCenterCategoryCell = 6;
    private const string DefaultCategoryName = "Undefined";
    private const char DocumentNumberPrefix = 'B';
    public async Task<Result> ImportTransactionsAsync(Stream? fileStream, string fileName, int cashRegisterId, CancellationToken ct = default)
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

            var cashRegister = await cashRegisterService.GetCashRegisterByIdAsync(cashRegisterId, ct);
            if (cashRegister == null)
            {
                logger.LogError("Cash register with ID {CashRegisterId} not found.", cashRegisterId);
                return operationResultFactory.NotFound(localizer["CashRegister"], cashRegisterId);
            }

            var importCounter = 0;
            var failedDocNumbers = new List<int>();
            var existingDocNumbers = await transactionService.GetAllDocumentNumbersAsync(cashRegisterId, ct);

            foreach (var row in parsedRows)
            {
                try
                {
                    ct.ThrowIfCancellationRequested();

                    if (existingDocNumbers.Contains(row.DocumentNumber))
                    {
                        logger.LogInformation(
                            "Skipping duplicate document number: {DocumentNumber}",
                            row.DocumentNumber);
                        continue;
                    }

                    var allocation = await allocationService.GetOrCreateAllocationAsync(
                        row.CostCenterName,
                        row.CategoryName,
                        ct: ct);

                    var transaction = new TransactionModel
                    {
                        CashRegisterId = cashRegisterId,
                        Date = row.Date,
                        Documentnumber = row.DocumentNumber,
                        Description = row.Description,
                        Sum = row.Sum,
                        AccountMovement = row.AccountMovement,
                        AllocationId = allocation.Id
                    };

                    var addResult = await transactionService.AddTransactionAsync(transaction, ct);
                    if (addResult.IsFailure)
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
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception rowEx)
                {
                    logger.LogError(rowEx, "Error processing row with document number {DocumentNumber}", row.DocumentNumber);
                    failedDocNumbers.Add(row.DocumentNumber);
                }
            }

            if (failedDocNumbers.Count > 0)
            {
                logger.LogWarning(
                    "Import completed with {FailedCount} failed rows. Failed document numbers: {FailedDocNumbers}",
                    failedDocNumbers.Count, string.Join(", ", failedDocNumbers));

                return operationResultFactory.ImportFailed(
                    $"{localizer["RowsFailedDuringImport"]}: {string.Join(", ", failedDocNumbers)}");
            }

            logger.LogInformation(
                "Booking journal successfully imported. Transactions: {Count}",
                importCounter);

            return operationResultFactory.ImportSuccessful(fileName);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Import of booking journal was canceled");
            return operationResultFactory.Canceled();
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

        using var reader = ExcelReaderFactory.CreateReader(memoryStream);
        var result = reader.AsDataSet();
        var dataTable = result.Tables["Bookings"];

        if (dataTable?.Rows != null)
            return (from DataRow row in dataTable.Rows select ParseRow(row))
                .Where(r => r != null)
                .Select(r => r!)
                .ToList();
        logger.LogWarning("No rows found in sheet: 'Bookings'!");
        return null;

    }

    private BookingJournalRowDto? ParseRow(DataRow row)
    {
        try
        {
            if (!DateTime.TryParse(row.ItemArray.ElementAtOrDefault(DateCell)?.ToString(), out var datum))
            {
                logger.LogWarning("Invalid date in row: {@RowData}", row.ItemArray);
                return null;
            }

            var documentRaw = row.ItemArray[DocumentCell]?.ToString()?.TrimStart(DocumentNumberPrefix);
            if (!int.TryParse(documentRaw, out var documentNumber))
            {
                logger.LogWarning("Invalid document number in row: {@RowData}", row.ItemArray);
                return null;
            }

            var description = row.ItemArray[DescriptionCell]?.ToString();

            var sumStr = row.ItemArray[SumIncomeCell]?.ToString()?.Trim();
            if (string.IsNullOrEmpty(sumStr))
                sumStr = row.ItemArray[SumOutcomeCell]?.ToString()?.Trim();

            if (!decimal.TryParse(sumStr, out var sumValue))
            {
                logger.LogWarning("Invalid sum in row: {@RowData}", row.ItemArray);
                return null;
            }

            var accountMovement = 0m;
            if (row.ItemArray[AccountMovementCell] != DBNull.Value &&
                decimal.TryParse(row.ItemArray[AccountMovementCell]?.ToString(), out var accMove))
            {
                accountMovement = accMove;
            }

            var costCenterCategory = row.ItemArray[CostCenterCategoryCell]?.ToString();
            var parts = costCenterCategory?.Split('/');
            if (parts != null && parts.Length != 0)
                return new BookingJournalRowDto
                {
                    Date = DateOnly.FromDateTime(datum),
                    DocumentNumber = documentNumber,
                    Description = description,
                    Sum = sumValue,
                    AccountMovement = accountMovement,
                    CostCenterName = parts[0].Trim(),
                    CategoryName = parts.Length >= 2 ? parts[1].Trim() : DefaultCategoryName
                };
            logger.LogWarning("Missing cost center info in row: {@RowData}", row.ItemArray);
            return null;

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
        if (rows.Count == 0)
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
    public Result? ErrorResult { get; init; }
}