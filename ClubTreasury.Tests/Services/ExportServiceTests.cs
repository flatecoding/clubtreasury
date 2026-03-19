using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.Export;
using ClubTreasury.Data.Export.Budget;
using ClubTreasury.Data.Export.Transaction;
using ClubTreasury.Data.Mapper;
using ClubTreasury.Data.Mapper.DTOs;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Tests.Services;

[TestFixture]
public class ExportServiceTests
{
    private ITransactionService _transactionService = null!;
    private ILogger<ExportService> _logger = null!;
    private IBudgetMapper _budgetMapper = null!;
    private ICsvBudgetWriter _csvWriter = null!;
    private IExcelBudgetWriter _excelWriter = null!;
    private IPdfTransactionRenderer _pdfRenderer = null!;
    private IResultFactory _resultFactory = null!;
    private IStringLocalizer<Resources.Translation> _localizer = null!;
    private ExportService _sut = null!;
    private string _testExportPath = null!;
    private IExportPathProvider _exportPathProvider = null!;
    private ICashRegisterLogoService _cashRegisterLogoService = null!;

    [SetUp]
    public void SetUp()
    {
        _transactionService = A.Fake<ITransactionService>();
        _logger = A.Fake<ILogger<ExportService>>();
        _budgetMapper = A.Fake<IBudgetMapper>();
        _csvWriter = A.Fake<ICsvBudgetWriter>();
        _excelWriter = A.Fake<IExcelBudgetWriter>();
        _pdfRenderer = A.Fake<IPdfTransactionRenderer>();
        _resultFactory = A.Fake<IResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Resources.Translation>>();
        _exportPathProvider = A.Fake<IExportPathProvider>();
        _cashRegisterLogoService = A.Fake<ICashRegisterLogoService>();

        _testExportPath = Path.Combine(Path.GetTempPath(), $"ExportTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testExportPath);
        A.CallTo(() => _exportPathProvider.ExportPath)
            .Returns(_testExportPath);  

        A.CallTo(() => _localizer["NoData"])
            .Returns(new LocalizedString("NoData", "No data available"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));

        _sut = new ExportService(
            _transactionService,
            _logger,
            _budgetMapper,
            _csvWriter,
            _excelWriter,
            _pdfRenderer,
            _resultFactory,
            _localizer,
            _exportPathProvider,
            _cashRegisterLogoService);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testExportPath))
        {
            Directory.Delete(_testExportPath, recursive: true);
        }
    }

    #region ExportTransactionsToCsv Tests

    [Test]
    public async Task ExportTransactionsToCsv_WhenTransactionsExist_ShouldCreateCsvAndReturnSuccess()
    {
        // Arrange
        var begin = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);
        const string filename = "test_export.csv";

        var transactions = new List<TransactionModel>
        {
            new() { Documentnumber = 1, Description = "Test 1", Sum = 100m, AccountMovement = 100m },
            new() { Documentnumber = 2, Description = "Test 2", Sum = 200m, AccountMovement = 200m }
        };

        A.CallTo(() => _transactionService.GetTransactionsForExportAsync(begin, end, 1))
            .Returns(transactions);

        var expectedResult = Result.Success("Export successful");

        A.CallTo(() => _resultFactory.ExportSuccessful(filename))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ExportTransactionsToCsvAsync(begin, end, filename, 1);

        // Assert
        result.Should().Be(expectedResult);

        var filePath = Path.Combine(_testExportPath, filename);
        File.Exists(filePath).Should().BeTrue();

        var lines = await File.ReadAllLinesAsync(filePath);
        lines.Should().HaveCount(3);
        lines[0].Should().Be("Belegnr.;Beschreibung;Rechnungsbetrag;Kontobewegung");
        lines[1].Should().Be("1;Test 1;100;100");
        lines[2].Should().Be("2;Test 2;200;200");
    }

    [Test]
    public async Task ExportTransactionsToCsv_WhenNoTransactions_ShouldReturnExportFailed()
    {
        var begin = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);

        A.CallTo(() => _transactionService.GetTransactionsForExportAsync(begin, end, 1))
            .Returns(new List<TransactionModel>());

        var failed = Result.Failure(new Error("Test.Error", "No data available"));

        A.CallTo(() => _resultFactory.ExportFailed(A<string>._))
            .Returns(failed);

        var result = await _sut.ExportTransactionsToCsvAsync(begin, end, "empty.csv", 1);

        result.Should().Be(failed);
    }

    [Test]
    public async Task ExportTransactionsToCsv_WhenExceptionOccurs_ShouldReturnExportFailed()
    {
        // Arrange
        var begin = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);
        var filename = "error_export.csv";

        A.CallTo(() => _transactionService.GetTransactionsForExportAsync(begin, end, 1))
            .Throws(new Exception("Database error"));

        var expectedResult = Result.Failure(new Error("Test.Error", "An error occurred"));
        A.CallTo(() => _resultFactory.ExportFailed(A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ExportTransactionsToCsvAsync(begin, end, filename, 1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region ExportTransactionsToPdf Tests

    [Test]
    public async Task ExportTransactionsToPdf_WhenSuccessful_ShouldReturnSuccess()
    {
        // Arrange
        var begin = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);
        var filename = "test_export.pdf";
        var cancellationToken = CancellationToken.None;

        var transactions = new List<TransactionModel>
        {
            new() { Documentnumber = 1, Description = "Test" }
        };

        A.CallTo(() => _transactionService.GetTransactionsForExportAsync(begin, end, 1))
            .Returns(transactions);

        var expectedResult = Result.Success("Export successful");
        A.CallTo(() => _resultFactory.ExportSuccessful(filename))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ExportTransactionsToPdfAsync(begin, end, filename, 1, "Test Register", cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _pdfRenderer.RenderTransactionPdfExportAsync(
            transactions, begin, end, A<string>._, "Test Register", A<byte[]?>._, A<string?>._, cancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task ExportTransactionsToPdf_WhenCanceled_ShouldReturnCanceled()
    {
        // Arrange
        var begin = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);
        var filename = "canceled_export.pdf";
        var cancellationToken = new CancellationToken(canceled: true);

        A.CallTo(() => _transactionService.GetTransactionsForBudgetExportAsync(begin, end, 1))
            .Returns(new List<TransactionModel>());

        A.CallTo(() => _pdfRenderer.RenderTransactionPdfExportAsync(
                A<IEnumerable<TransactionModel>>._, begin, end, A<string>._, A<string>._, A<byte[]?>._, A<string?>._, cancellationToken))
            .Throws(new OperationCanceledException());

        var expectedResult = Result.Failure(Error.Canceled with { Message = "Canceled" });
        A.CallTo(() => _resultFactory.Canceled())
            .Returns(expectedResult);

        // Act
        var result = await _sut.ExportTransactionsToPdfAsync(begin, end, filename, 1, "Test Register", cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task ExportTransactionsToPdf_WhenExceptionOccurs_ShouldReturnExportFailed()
    {
        // Arrange
        var begin = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);
        var filename = "error_export.pdf";
        var cancellationToken = CancellationToken.None;

        A.CallTo(() => _transactionService.GetTransactionsForExportAsync(begin, end, 1))
            .Throws(new Exception("Render error"));

        var expectedResult = Result.Failure(new Error("Test.Error", "An error occurred"));
        A.CallTo(() => _resultFactory.ExportFailed(A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ExportTransactionsToPdfAsync(begin, end, filename, 1, "Test Register", cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region ExportBudgetToCsv Tests

    [Test]
    public async Task ExportBudgetToCsv_WhenSuccessful_ShouldReturnSuccess()
    {
        // Arrange
        var begin = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);
        var filename = "budget_export.csv";

        var transactions = new List<TransactionModel>
        {
            new() { Documentnumber = 1, Description = "Test" }
        };
        var flatEntries = new List<BudgetFlatEntryDto>();
        var grouped = new List<BudgetGroupedDto>();

        A.CallTo(() => _transactionService.GetTransactionsForBudgetExportAsync(begin, end, 1))
            .Returns(transactions);
        A.CallTo(() => _budgetMapper.BuildFlatEntries(transactions))
            .Returns(flatEntries);
        A.CallTo(() => _budgetMapper.BuildBudgetHierarchy(flatEntries))
            .Returns(grouped);

        var expectedResult = Result.Success("Export successful");
        A.CallTo(() => _resultFactory.ExportSuccessful(filename))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ExportBudgetToCsvAsync(begin, end, filename, 1);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _csvWriter.WriteAsync(A<string>._, grouped))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task ExportBudgetToCsv_WhenExceptionOccurs_ShouldReturnExportFailed()
    {
        // Arrange
        var begin = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);
        var filename = "error_budget.csv";

        A.CallTo(() => _transactionService.GetTransactionsForBudgetExportAsync(begin, end, 1))
            .Throws(new Exception("Database error"));

        var expectedResult = Result.Failure(new Error("Test.Error", "Database error"));
        A.CallTo(() => _resultFactory.ExportFailed(A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ExportBudgetToCsvAsync(begin, end, filename, 1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region ExportBudgetToExcel Tests

    [Test]
    public async Task ExportBudgetToExcel_WhenSuccessful_ShouldReturnSuccess()
    {
        // Arrange
        var begin = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);
        var filename = "budget_export.xlsx";

        var transactions = new List<TransactionModel>
        {
            new() { Documentnumber = 1, Description = "Test" }
        };
        var flatEntries = new List<BudgetFlatEntryDto>();
        var grouped = new List<BudgetGroupedDto>();

        A.CallTo(() => _transactionService.GetTransactionsForBudgetExportAsync(begin, end, 1))
            .Returns(transactions);
        A.CallTo(() => _budgetMapper.BuildFlatEntries(transactions))
            .Returns(flatEntries);
        A.CallTo(() => _budgetMapper.BuildBudgetHierarchy(flatEntries))
            .Returns(grouped);

        var expectedResult = Result.Success("Export successful");
        A.CallTo(() => _resultFactory.ExportSuccessful(filename))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ExportBudgetToExcelAsync(begin, end, filename, 1);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _excelWriter.WriteAsync(A<string>._, grouped, begin, end))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task ExportBudgetToExcel_WhenExceptionOccurs_ShouldReturnExportFailed()
    {
        // Arrange
        var begin = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);
        var filename = "error_budget.xlsx";

        A.CallTo(() => _transactionService.GetTransactionsForBudgetExportAsync(begin, end, 1))
            .Throws(new Exception("Database error"));

        var expectedResult = Result.Failure(new Error("Test.Error", "Database error"));
        A.CallTo(() => _resultFactory.ExportFailed(A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ExportBudgetToExcelAsync(begin, end, filename, 1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region ExportBudgetToExcelBytes Tests

    [Test]
    public async Task ExportBudgetToExcelBytes_WhenSuccessful_ShouldReturnFileBytes()
    {
        // Arrange
        var begin = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);

        var transactions = new List<TransactionModel>();
        var flatEntries = new List<BudgetFlatEntryDto>();
        var grouped = new List<BudgetGroupedDto>();

        A.CallTo(() => _transactionService.GetTransactionsForBudgetExportAsync(begin, end, 1))
            .Returns(transactions);

        A.CallTo(() => _budgetMapper.BuildFlatEntries(transactions))
            .Returns(flatEntries);

        A.CallTo(() => _budgetMapper.BuildBudgetHierarchy(flatEntries))
            .Returns(grouped);

        var success = Result.Success();

        A.CallTo(() => _resultFactory.ExportSuccessful(A<string>._))
            .Returns(success);

        var expectedBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 };

        A.CallTo(() => _excelWriter.WriteAsync(
                A<string>._, grouped, begin, end))
            .Invokes((string path, IEnumerable<BudgetGroupedDto> _, DateTime _, DateTime _) =>
            {
                File.WriteAllBytes(path, expectedBytes);
            });

        // Act
        var result = await _sut.ExportBudgetToExcelBytesAsync(begin, end, 1);

        // Assert
        result.Should().BeEquivalentTo(expectedBytes);
    }

    [Test]
    public async Task ExportBudgetToExcelBytes_WhenExportFails_ShouldReturnEmptyArray()
    {
        var begin = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);

        A.CallTo(() => _transactionService.GetTransactionsForBudgetExportAsync(begin, end, 1))
            .Throws(new Exception("Error"));

        var failed = Result.Failure(new Error("Test.Error", "Failed"));

        A.CallTo(() => _resultFactory.ExportFailed(A<string>._))
            .Returns(failed);

        var result = await _sut.ExportBudgetToExcelBytesAsync(begin, end, 1);

        result.Should().BeEmpty();
    }


    #endregion
}