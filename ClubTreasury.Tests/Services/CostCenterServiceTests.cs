using FakeItEasy;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TTCCashRegister.Data;
using TTCCashRegister.Data.CostCenter;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Tests.Services;

[TestFixture]
public class CostCenterServiceTests
{
    private CashDataContext _context = null!;
    private ILogger<CostCenterService> _logger = null!;
    private IOperationResultFactory _operationResultFactory = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private CostCenterService _sut = null!;
    private bool _contextDisposed;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CashDataContext(options);
        _contextDisposed = false;
        _logger = A.Fake<ILogger<CostCenterService>>();
        _operationResultFactory = A.Fake<IOperationResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();

        A.CallTo(() => _localizer["CostCenter"])
            .Returns(new LocalizedString("CostCenter", "Cost Center"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));

        _sut = new CostCenterService(_context, _logger, _localizer, _operationResultFactory);
    }

    [TearDown]
    public void TearDown()
    {
        if (_contextDisposed) return;

        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetAllCostCentersAsync Tests

    [Test]
    public async Task GetAllCostCentersAsync_WhenNoCostCentersExist_ShouldReturnEmptyList()
    {
        // Act
        var result = await _sut.GetAllCostCentersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllCostCentersAsync_WhenCostCentersExist_ShouldReturnAllCostCentersOrderedById()
    {
        // Arrange
        var costCenters = new List<CostCenterModel>
        {
            new() { CostUnitName = "Marketing" },
            new() { CostUnitName = "Development" },
            new() { CostUnitName = "Sales" }
        };
        await _context.CostCenters.AddRangeAsync(costCenters);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllCostCentersAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Select(c => c.CostUnitName).Should().ContainInOrder("Marketing", "Development", "Sales");
    }

    #endregion

    #region GetCostCenterByIdAsync Tests

    [Test]
    public async Task GetCostCenterByIdAsync_WhenCostCenterExists_ShouldReturnCostCenter()
    {
        // Arrange
        var costCenter = new CostCenterModel { CostUnitName = "Test Cost Center" };
        await _context.CostCenters.AddAsync(costCenter);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCostCenterByIdAsync(costCenter.Id);

        // Assert
        result.Should().NotBeNull();
        result.CostUnitName.Should().Be("Test Cost Center");
    }

    [Test]
    public async Task GetCostCenterByIdAsync_WhenCostCenterDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetCostCenterByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetCostCenterByNameAsync Tests

    [Test]
    public async Task GetCostCenterByNameAsync_WhenCostCenterExists_ShouldReturnCostCenter()
    {
        // Arrange
        var costCenter = new CostCenterModel { CostUnitName = "Marketing" };
        await _context.CostCenters.AddAsync(costCenter);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCostCenterByNameAsync("Marketing");

        // Assert
        result.Should().NotBeNull();
        result.CostUnitName.Should().Be("Marketing");
    }

    [Test]
    public async Task GetCostCenterByNameAsync_WhenCostCenterDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetCostCenterByNameAsync("NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetCostCenterByNameAsync_WhenNameDoesNotMatchExactly_ShouldReturnNull()
    {
        // Arrange
        var costCenter = new CostCenterModel { CostUnitName = "Marketing" };
        await _context.CostCenters.AddAsync(costCenter);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCostCenterByNameAsync("marketing"); // lowercase

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddCostCenterAsync Tests

    [Test]
    public async Task AddCostCenterAsync_WhenValidCostCenter_ShouldAddAndReturnSuccess()
    {
        // Arrange
        var costCenter = new CostCenterModel { CostUnitName = "New Cost Center" };
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully added"
        };
        A.CallTo(() => _operationResultFactory.SuccessAdded(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddCostCenterAsync(costCenter);

        // Assert
        result.Should().Be(expectedResult);
        var addedCostCenter = await _context.CostCenters.FirstOrDefaultAsync(c => c.CostUnitName == "New Cost Center");
        addedCostCenter.Should().NotBeNull();
        A.CallTo(() => _operationResultFactory.SuccessAdded(
            A<string>.That.Contains("New Cost Center"),
            A<object?>._)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task AddCostCenterAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to add"
        };
        A.CallTo(() => _operationResultFactory.FailedToAdd(A<string>._, A<string?>._))
            .Returns(expectedResult);

        // Dispose context to simulate an error
        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new CostCenterService(disposedContext, _logger, _localizer, _operationResultFactory);

        var costCenter = new CostCenterModel { CostUnitName = "New Cost Center" };

        // Act
        var result = await _sut.AddCostCenterAsync(costCenter);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region UpdateCostCenterAsync Tests

    [Test]
    public async Task UpdateCostCenterAsync_WhenValidCostCenter_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var costCenter = new CostCenterModel { CostUnitName = "Original Name" };
        await _context.CostCenters.AddAsync(costCenter);
        await _context.SaveChangesAsync();

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully updated"
        };
        A.CallTo(() => _operationResultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);

        costCenter.CostUnitName = "Updated Name";

        // Act
        var result = await _sut.UpdateCostCenterAsync(costCenter);

        // Assert
        result.Should().Be(expectedResult);
        var updatedCostCenter = await _context.CostCenters.FindAsync(costCenter.Id);
        updatedCostCenter!.CostUnitName.Should().Be("Updated Name");
    }

    [Test]
    public async Task UpdateCostCenterAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to update"
        };
        A.CallTo(() => _operationResultFactory.FailedToUpdate(A<string>._, A<string?>._))
            .Returns(expectedResult);

        // Dispose context to simulate error
        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new CostCenterService(disposedContext, _logger, _localizer, _operationResultFactory);

        var costCenter = new CostCenterModel { CostUnitName = "Test" };

        // Act
        var result = await _sut.UpdateCostCenterAsync(costCenter);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region DeleteCostCenterAsync Tests

    [Test]
    public async Task DeleteCostCenterAsync_WhenCostCenterExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var costCenter = new CostCenterModel { CostUnitName = "To Be Deleted" };
        await _context.CostCenters.AddAsync(costCenter);
        await _context.SaveChangesAsync();
        var id = costCenter.Id;

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully deleted"
        };
        A.CallTo(() => _operationResultFactory.SuccessDeleted(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteCostCenterAsync(id);

        // Assert
        result.Should().Be(expectedResult);
        var deletedCostCenter = await _context.CostCenters.FindAsync(id);
        deletedCostCenter.Should().BeNull();
    }

    [Test]
    public async Task DeleteCostCenterAsync_WhenCostCenterDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Not found"
        };
        A.CallTo(() => _operationResultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteCostCenterAsync(999);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _operationResultFactory.NotFound(A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteCostCenterAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to delete"
        };
        A.CallTo(() => _operationResultFactory.FailedToDelete(A<string>._, A<string?>._))
            .Returns(expectedResult);

        // Dispose context to simulate error during delete
        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new CostCenterService(disposedContext, _logger, _localizer, _operationResultFactory);

        // Act
        var result = await _sut.DeleteCostCenterAsync(1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Constructor Tests

    [Test]
    public void Constructor_WhenContextIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new CostCenterService(null!, _logger, _localizer, _operationResultFactory);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    #endregion
}