using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.CashRegister;
using TTCCashRegister.Data.Import;
using TTCCashRegister.Data.OperationResult;
using TTCCashRegister.Data.Transaction;

namespace TTCCashRegister.Tests.Services;

[TestFixture]
public class ImportBookingJournalServiceTests
{
    private ITransactionService _transactionService = null!;
    private IAllocationService _allocationService = null!;
    private ICashRegisterService _cashRegisterService = null!;
    private ILogger<ImportBookingJournalService> _logger = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private IOperationResultFactory _operationResultFactory = null!;
    private ImportBookingJournalService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _transactionService = A.Fake<ITransactionService>();
        _allocationService = A.Fake<IAllocationService>();
        _cashRegisterService = A.Fake<ICashRegisterService>();
        _logger = A.Fake<ILogger<ImportBookingJournalService>>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();
        _operationResultFactory = A.Fake<IOperationResultFactory>();

        SetupLocalizer();

        _sut = new ImportBookingJournalService(
            _transactionService,
            _allocationService,
            _cashRegisterService,
            _logger,
            _localizer,
            _operationResultFactory);
    }

    private void SetupLocalizer()
    {
        A.CallTo(() => _localizer["FileStreamError"])
            .Returns(new LocalizedString("FileStreamError", "File stream error"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));
        A.CallTo(() => _localizer["NoData"])
            .Returns(new LocalizedString("NoData", "No data"));
        A.CallTo(() => _localizer["CashRegister"])
            .Returns(new LocalizedString("CashRegister", "Cash Register"));
        A.CallTo(() => _localizer["DocumentNumberError"])
            .Returns(new LocalizedString("DocumentNumberError", "Document number error"));
        A.CallTo(() => _localizer["DateMissing"])
            .Returns(new LocalizedString("DateMissing", "Date missing"));
        A.CallTo(() => _localizer["DateInFuture"])
            .Returns(new LocalizedString("DateInFuture", "Date in future"));
        A.CallTo(() => _localizer["InvalidDocumentNumber"])
            .Returns(new LocalizedString("InvalidDocumentNumber", "Invalid document number"));
        A.CallTo(() => _localizer["InvalidSum"])
            .Returns(new LocalizedString("InvalidSum", "Invalid sum"));
        A.CallTo(() => _localizer["InvalidAccountMovement"])
            .Returns(new LocalizedString("InvalidAccountMovement", "Invalid account movement"));
        A.CallTo(() => _localizer["SumAccountMismatch"])
            .Returns(new LocalizedString("SumAccountMismatch", "Sum and account movement mismatch"));
        A.CallTo(() => _localizer["CostCenterMissing"])
            .Returns(new LocalizedString("CostCenterMissing", "Cost center missing"));
        A.CallTo(() => _localizer["CategoryMissing"])
            .Returns(new LocalizedString("CategoryMissing", "Category missing"));
        A.CallTo(() => _localizer["DuplicateRow"])
            .Returns(new LocalizedString("DuplicateRow", "Duplicate row"));
        A.CallTo(() => _localizer["RowErrorFormat"])
            .Returns(new LocalizedString("RowErrorFormat", "Row {0}: {1}"));
    }

    private static MemoryStream CreateExcelStream(Action<ExcelWorksheet> configureSheet)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Test");
        var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Bookings");
        configureSheet(worksheet);

        var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    #region ImportTransactions Tests

    [Test]
    public async Task ImportTransactions_WhenFileStreamIsNull_ShouldReturnImportFailed()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "File stream error"
        };
        A.CallTo(() => _operationResultFactory.ImportFailed(A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ImportTransactions(null, "test.xlsx", 1);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _transactionService.AddTransactionAsync(A<TransactionModel>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task ImportTransactions_WhenValidExcelFile_ShouldImportTransactionsAndReturnSuccess()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        using var stream = CreateExcelStream(ws =>
        {
            ws.Cells[1, 1].Value = today.ToString("yyyy-MM-dd");
            ws.Cells[1, 2].Value = "B100";
            ws.Cells[1, 3].Value = "Test Description";
            ws.Cells[1, 4].Value = 50.00m;
            ws.Cells[1, 5].Value = "";
            ws.Cells[1, 6].Value = 50.00m;
            ws.Cells[1, 7].Value = "Marketing/Advertising";
        });
        const string fileName = "bookings.xlsx";

        A.CallTo(() => _transactionService.GetAllDocumentNumbersAsync(1))
            .Returns(new HashSet<int>());

        var allocation = new AllocationModel { Id = 1 };
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
                A<string>._, A<string>._, A<string?>._))
            .Returns(allocation);

        var addResult = new OperationResult { Status = OperationResultStatus.Success };
        A.CallTo(() => _transactionService.AddTransactionAsync(A<TransactionModel>._, A<CancellationToken>._))
            .Returns(addResult);

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Import successful"
        };
        A.CallTo(() => _operationResultFactory.ImportSuccessful(fileName))
            .Returns(expectedResult);
        
        TransactionModel? importedTransaction = null;
        A.CallTo(() => _transactionService.AddTransactionAsync(
                A<TransactionModel>._, A<CancellationToken>._))
            .Invokes((TransactionModel tx, CancellationToken _) =>
            {
                importedTransaction = tx;
            })
            .Returns(addResult);

        // Act
        var result = await _sut.ImportTransactions(stream, fileName, 1);

        // Assert
        result.Should().Be(expectedResult);
        importedTransaction.Should().NotBeNull();
        importedTransaction!.Date.Should().Be(today);
        importedTransaction.Documentnumber.Should().Be(100);
        importedTransaction.Description.Should().Be("Test Description");
        importedTransaction.Sum.Should().Be(50.00m);
        importedTransaction.AccountMovement.Should().Be(50.00m);
        importedTransaction.AllocationId.Should().Be(allocation.Id);
        importedTransaction.CashRegisterId.Should().Be(1);
        
        A.CallTo(() => _transactionService.AddTransactionAsync(A<TransactionModel>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task ImportTransactions_WhenNoCashRegisterExists_ShouldReturnNotFound()
    {
        // Arrange
        var today = DateTime.Today;
        using var stream = CreateExcelStream(ws =>
        {
            ws.Cells[1, 1].Value = today.ToString("yyyy-MM-dd");
            ws.Cells[1, 2].Value = "B100";
            ws.Cells[1, 3].Value = "Test";
            ws.Cells[1, 4].Value = 50.00m;
            ws.Cells[1, 5].Value = "";
            ws.Cells[1, 6].Value = 50.00m;
            ws.Cells[1, 7].Value = "Marketing/Advertising";
        });

        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1))
            .Returns((CashRegisterModel?)null);

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Not found"
        };
        A.CallTo(() => _operationResultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ImportTransactions(stream, "bookings.xlsx", 1);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task ImportTransactions_WhenDocumentNumberAlreadyExists_ShouldSkipDuplicates()
    {
        // Arrange
        var today = DateTime.Today;
        using var stream = CreateExcelStream(ws =>
        {
            // Row 1 - will be skipped (existing)
            ws.Cells[1, 1].Value = today.ToString("yyyy-MM-dd");
            ws.Cells[1, 2].Value = "B100";
            ws.Cells[1, 3].Value = "Existing";
            ws.Cells[1, 4].Value = 50.00m;
            ws.Cells[1, 5].Value = "";
            ws.Cells[1, 6].Value = 50.00m;
            ws.Cells[1, 7].Value = "Marketing/Advertising";

            // Row 2 - will be imported
            ws.Cells[2, 1].Value = today.ToString("yyyy-MM-dd");
            ws.Cells[2, 2].Value = "B200";
            ws.Cells[2, 3].Value = "New";
            ws.Cells[2, 4].Value = 75.00m;
            ws.Cells[2, 5].Value = "";
            ws.Cells[2, 6].Value = 75.00m;
            ws.Cells[2, 7].Value = "Sales/Revenue";
        });
        const string fileName = "bookings.xlsx";

        A.CallTo(() => _transactionService.GetAllDocumentNumbersAsync(1))
            .Returns([100]); // Document 100 already exists

        var allocation = new AllocationModel { Id = 1 };
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
                A<string>._, A<string>._, A<string?>._))
            .Returns(allocation);

        var addResult = new OperationResult { Status = OperationResultStatus.Success };
        A.CallTo(() => _transactionService.AddTransactionAsync(A<TransactionModel>._, A<CancellationToken>._))
            .Returns(addResult);

        var expectedResult = new OperationResult { Status = OperationResultStatus.Success };
        A.CallTo(() => _operationResultFactory.ImportSuccessful(fileName))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ImportTransactions(stream, fileName, 1);

        // Assert
        result.Should().Be(expectedResult);
        // Only one transaction should be added (doc 200), doc 100 was skipped
        A.CallTo(() => _transactionService.AddTransactionAsync(
                A<TransactionModel>.That.Matches(t => t.Documentnumber == 200),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task ImportTransactions_WhenAddTransactionFails_ShouldReturnImportFailed()
    {
        // Arrange
        var today = DateTime.Today;
        using var stream = CreateExcelStream(ws =>
        {
            ws.Cells[1, 1].Value = today.ToString("yyyy-MM-dd");
            ws.Cells[1, 2].Value = "B100";
            ws.Cells[1, 3].Value = "Test";
            ws.Cells[1, 4].Value = 50.00m;
            ws.Cells[1, 5].Value = "";
            ws.Cells[1, 6].Value = 50.00m;
            ws.Cells[1, 7].Value = "Marketing/Advertising";
        });

        A.CallTo(() => _transactionService.GetAllDocumentNumbersAsync(1))
            .Returns(new HashSet<int>());

        var allocation = new AllocationModel { Id = 1 };
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
                A<string>._, A<string>._, A<string?>._))
            .Returns(allocation);

        var addResult = new OperationResult { Status = OperationResultStatus.Failed };
        A.CallTo(() => _transactionService.AddTransactionAsync(A<TransactionModel>._, A<CancellationToken>._))
            .Returns(addResult);

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Import failed"
        };
        A.CallTo(() => _operationResultFactory.ImportFailed(A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ImportTransactions(stream, "bookings.xlsx", 1);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task ImportTransactions_WhenEmptyExcelFile_ShouldReturnNoData()
    {
        // Arrange
        using var stream = CreateExcelStream(_ => { }); // Empty sheet

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "No data"
        };
        A.CallTo(() => _operationResultFactory.ImportFailed(A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ImportTransactions(stream, "empty.xlsx", 1);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task ImportTransactions_WhenExceptionOccurs_ShouldReturnImportFailed()
    {
        // Arrange
        var today = DateTime.Today;
        using var stream = CreateExcelStream(ws =>
        {
            ws.Cells[1, 1].Value = today.ToString("yyyy-MM-dd");
            ws.Cells[1, 2].Value = "B100";
            ws.Cells[1, 3].Value = "Test";
            ws.Cells[1, 4].Value = 50.00m;
            ws.Cells[1, 5].Value = "";
            ws.Cells[1, 6].Value = 50.00m;
            ws.Cells[1, 7].Value = "Marketing/Advertising";
        });

        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1))
            .Throws(new Exception("Database error"));

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "An error occurred"
        };
        A.CallTo(() => _operationResultFactory.ImportFailed(A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ImportTransactions(stream, "bookings.xlsx", 1);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task ImportTransactions_WhenRowHasInvalidDate_ShouldSkipRow()
    {
        // Arrange
        var today = DateTime.Today;
        using var stream = CreateExcelStream(ws =>
        {
            // Invalid date row
            ws.Cells[1, 1].Value = "invalid-date";
            ws.Cells[1, 2].Value = "B100";
            ws.Cells[1, 3].Value = "Test";
            ws.Cells[1, 4].Value = 50.00m;
            ws.Cells[1, 5].Value = "";
            ws.Cells[1, 6].Value = 50.00m;
            ws.Cells[1, 7].Value = "Marketing/Advertising";

            // Valid row
            ws.Cells[2, 1].Value = today.ToString("yyyy-MM-dd");
            ws.Cells[2, 2].Value = "B200";
            ws.Cells[2, 3].Value = "Valid";
            ws.Cells[2, 4].Value = 75.00m;
            ws.Cells[2, 5].Value = "";
            ws.Cells[2, 6].Value = 75.00m;
            ws.Cells[2, 7].Value = "Sales/Revenue";
        });
        const string fileName = "bookings.xlsx";

        A.CallTo(() => _transactionService.GetAllDocumentNumbersAsync(1))
            .Returns(new HashSet<int>());

        var allocation = new AllocationModel { Id = 1 };
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
                A<string>._, A<string>._, A<string?>._))
            .Returns(allocation);

        var addResult = new OperationResult { Status = OperationResultStatus.Success };
        A.CallTo(() => _transactionService.AddTransactionAsync(A<TransactionModel>._, A<CancellationToken>._))
            .Returns(addResult);

        var expectedResult = new OperationResult { Status = OperationResultStatus.Success };
        A.CallTo(() => _operationResultFactory.ImportSuccessful(fileName))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ImportTransactions(stream, fileName, 1);

        // Assert
        result.Should().Be(expectedResult);
        // Only the valid row should be imported
        A.CallTo(() => _transactionService.AddTransactionAsync(
                A<TransactionModel>.That.Matches(t => t.Documentnumber == 200),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion
}