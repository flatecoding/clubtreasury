using System.Text;
using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.Import;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Tests.Services;

[TestFixture]
public class ImportCostCenterServiceTests
{
    private IAllocationService _allocationService = null!;
    private ILogger<ImportCostCenterService> _logger = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private IResultFactory _resultFactory = null!;
    private ImportCostCenterService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _allocationService = A.Fake<IAllocationService>();
        _logger = A.Fake<ILogger<ImportCostCenterService>>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();
        _resultFactory = A.Fake<IResultFactory>();

        A.CallTo(() => _localizer["FileStreamError"])
            .Returns(new LocalizedString("FileStreamError", "File stream error"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));
        A.CallTo(() => _localizer["Undefined"])
            .Returns(new LocalizedString("Undefined", "Undefined"));

        _sut = new ImportCostCenterService(
            _allocationService,
            _logger,
            _localizer,
            _resultFactory);
    }

    private static MemoryStream CreateFileStream(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new MemoryStream(bytes);
    }

    #region ImportCostCentersAndPositions Tests

    [Test]
    public async Task ImportCostCentersAndPositions_WhenFileStreamIsNull_ShouldReturnImportFailed()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "File stream error"));
        A.CallTo(() => _resultFactory.ImportFailed(A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ImportCostCentersAndPositionsAsync(null, "test.txt");

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
            A<string>._, A<string>._, A<string?>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task ImportCostCentersAndPositions_WhenValidFile_ShouldImportAllLinesAndReturnSuccess()
    {
        // Arrange
        const string fileContent = """
            Marketing/Advertising
            Sales/Commission
            Development/Software
            """;
        using var stream = CreateFileStream(fileContent);
        const string fileName = "costcenters.txt";

        var expectedResult = Result.Success("Import successful");
        A.CallTo(() => _resultFactory.ImportSuccessful(fileName))
            .Returns(expectedResult);

        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
                A<string>._, A<string>._, A<string?>._))
            .Returns(new AllocationModel());

        // Act
        var result = await _sut.ImportCostCentersAndPositionsAsync(stream, fileName);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
            "Marketing", "Advertising", A<string?>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
            "Sales", "Commission", A<string?>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
            "Development", "Software", A<string?>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task ImportCostCentersAndPositions_WhenLineHasNoCategoryPart_ShouldUseUndefinedCategory()
    {
        // Arrange
        const string fileContent = "Marketing";
        using var stream = CreateFileStream(fileContent);
        const string fileName = "costcenters.txt";

        var expectedResult = Result.Success("Import successful");
        A.CallTo(() => _resultFactory.ImportSuccessful(fileName))
            .Returns(expectedResult);

        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
                A<string>._, A<string>._, A<string?>._))
            .Returns(new AllocationModel());

        // Act
        var result = await _sut.ImportCostCentersAndPositionsAsync(stream, fileName);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
            "Marketing", "Undefined", A<string?>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task ImportCostCentersAndPositions_WhenFileHasEmptyLines_ShouldSkipEmptyLines()
    {
        // Arrange
        const string fileContent = """
            Marketing/Advertising

            Sales/Commission

            """;
        using var stream = CreateFileStream(fileContent);
        const string fileName = "costcenters.txt";

        var expectedResult = Result.Success("Import successful");
        A.CallTo(() => _resultFactory.ImportSuccessful(fileName))
            .Returns(expectedResult);

        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
                A<string>._, A<string>._, A<string?>._))
            .Returns(new AllocationModel());

        // Act
        var result = await _sut.ImportCostCentersAndPositionsAsync(stream, fileName);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
            A<string>._, A<string>._, A<string?>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Test]
    public async Task ImportCostCentersAndPositions_WhenFileIsEmpty_ShouldReturnSuccessWithZeroImports()
    {
        // Arrange
        const string fileContent = "";
        using var stream = CreateFileStream(fileContent);
        const string fileName = "empty.txt";

        var expectedResult = Result.Success("Import successful");
        A.CallTo(() => _resultFactory.ImportSuccessful(fileName))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ImportCostCentersAndPositionsAsync(stream, fileName);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
            A<string>._, A<string>._, A<string?>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task ImportCostCentersAndPositions_WhenLineHasExtraSlashes_ShouldOnlyUseCostCenterAndCategory()
    {
        // Arrange
        const string fileContent = "Marketing/Advertising/Extra/Parts";
        using var stream = CreateFileStream(fileContent);
        const string fileName = "costcenters.txt";

        var expectedResult = Result.Success("Import successful");
        A.CallTo(() => _resultFactory.ImportSuccessful(fileName))
            .Returns(expectedResult);

        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
                A<string>._, A<string>._, A<string?>._))
            .Returns(new AllocationModel());

        // Act
        var result = await _sut.ImportCostCentersAndPositionsAsync(stream, fileName);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
            "Marketing", "Advertising", A<string?>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task ImportCostCentersAndPositions_WhenLineHasWhitespace_ShouldTrimValues()
    {
        // Arrange
        const string fileContent = "  Marketing  /  Advertising  ";
        using var stream = CreateFileStream(fileContent);
        const string fileName = "costcenters.txt";

        var expectedResult = Result.Success("Import successful");
        A.CallTo(() => _resultFactory.ImportSuccessful(fileName))
            .Returns(expectedResult);

        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
                A<string>._, A<string>._, A<string?>._))
            .Returns(new AllocationModel());

        // Act
        var result = await _sut.ImportCostCentersAndPositionsAsync(stream, fileName);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
            "Marketing", "Advertising", A<string?>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task ImportCostCentersAndPositions_WhenExceptionOccurs_ShouldReturnImportFailed()
    {
        // Arrange
        const string fileContent = "Marketing/Advertising";
        using var stream = CreateFileStream(fileContent);
        const string fileName = "costcenters.txt";

        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(
                A<string>._, A<string>._, A<string?>._))
            .Throws(new Exception("Database error"));

        var expectedResult = Result.Failure(new Error("Test.Error", "An error occurred"));
        A.CallTo(() => _resultFactory.ImportFailed(A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.ImportCostCentersAndPositionsAsync(stream, fileName);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion
}