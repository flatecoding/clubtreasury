using FakeItEasy;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TTCCashRegister.Data;
using TTCCashRegister.Data.OperationResult;
using TTCCashRegister.Data.SpecialItem;

namespace TTCCashRegister.Tests.Services;

[TestFixture]
public class SpecialItemServiceTests
{
    private CashDataContext _context = null!;
    private ILogger<SpecialItemService> _logger = null!;
    private IOperationResultFactory _operationResultFactory = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private SpecialItemService _sut = null!;
    private bool _contextDisposed;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CashDataContext(options);
        _contextDisposed = false;
        _logger = A.Fake<ILogger<SpecialItemService>>();
        _operationResultFactory = A.Fake<IOperationResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();

        A.CallTo(() => _localizer["SpecialPosition"])
            .Returns(new LocalizedString("SpecialPosition", "Special Position"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));

        _sut = new SpecialItemService(_context, _logger, _localizer, _operationResultFactory);
    }

    [TearDown]
    public void TearDown()
    {
        if (_contextDisposed) return;

        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetAllSpecialItems Tests

    [Test]
    public async Task GetAllSpecialItems_WhenNoSpecialItemsExist_ShouldReturnEmptyList()
    {
        // Act
        var result = await _sut.GetAllSpecialItems();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllSpecialItems_WhenSpecialItemsExist_ShouldReturnAllSpecialItems()
    {
        // Arrange
        var specialItems = new List<SpecialItemModel>
        {
            new() { Name = "Deposit" },
            new() { Name = "Withdrawal" },
            new() { Name = "Transfer" }
        };
        await _context.SpecialItems.AddRangeAsync(specialItems);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllSpecialItems();

        // Assert
        result.Should().HaveCount(3);
        result.Select(s => s.Name).Should().BeEquivalentTo(["Deposit", "Withdrawal", "Transfer"]);
    }

    #endregion

    #region GetSpecialPositionById Tests

    [Test]
    public async Task GetSpecialPositionById_WhenSpecialItemExists_ShouldReturnSpecialItem()
    {
        // Arrange
        var specialItem = new SpecialItemModel { Name = "Test Special Item" };
        await _context.SpecialItems.AddAsync(specialItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetSpecialPositionById(specialItem.Id);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Special Item");
    }

    [Test]
    public async Task GetSpecialPositionById_WhenSpecialItemDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetSpecialPositionById(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddSpecialPositionAsync Tests

    [Test]
    public async Task AddSpecialPositionAsync_WhenValidSpecialItem_ShouldAddAndReturnSuccess()
    {
        // Arrange
        var specialItem = new SpecialItemModel { Name = "New Special Item" };
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully added"
        };
        A.CallTo(() => _operationResultFactory.SuccessAdded(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddSpecialPositionAsync(specialItem);

        // Assert
        result.Should().Be(expectedResult);
        var addedItem = await _context.SpecialItems.FirstOrDefaultAsync(s => s.Name == "New Special Item");
        addedItem.Should().NotBeNull();
        A.CallTo(() => _operationResultFactory.SuccessAdded(
            A<string>.That.Contains("New Special Item"),
            A<object?>._)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task AddSpecialPositionAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to add"
        };
        A.CallTo(() => _operationResultFactory.FailedToAdd(A<string>._, A<string?>._))
            .Returns(expectedResult);

        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new SpecialItemService(disposedContext, _logger, _localizer, _operationResultFactory);

        var specialItem = new SpecialItemModel { Name = "New Special Item" };

        // Act
        var result = await _sut.AddSpecialPositionAsync(specialItem);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region UpdateSpecialPositionAsync Tests

    [Test]
    public async Task UpdateSpecialPositionAsync_WhenValidSpecialItem_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var specialItem = new SpecialItemModel { Name = "Original Name" };
        await _context.SpecialItems.AddAsync(specialItem);
        await _context.SaveChangesAsync();

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully updated"
        };
        A.CallTo(() => _operationResultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);

        specialItem.Name = "Updated Name";

        // Act
        var result = await _sut.UpdateSpecialPositionAsync(specialItem);

        // Assert
        result.Should().Be(expectedResult);
        var updatedItem = await _context.SpecialItems.FindAsync(specialItem.Id);
        updatedItem!.Name.Should().Be("Updated Name");
    }

    [Test]
    public async Task UpdateSpecialPositionAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to update"
        };
        A.CallTo(() => _operationResultFactory.FailedToUpdate(A<string>._, A<string?>._))
            .Returns(expectedResult);

        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new SpecialItemService(disposedContext, _logger, _localizer, _operationResultFactory);

        var specialItem = new SpecialItemModel { Name = "Test" };

        // Act
        var result = await _sut.UpdateSpecialPositionAsync(specialItem);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region DeleteSpecialPositionAsync Tests

    [Test]
    public async Task DeleteSpecialPositionAsync_WhenSpecialItemExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var specialItem = new SpecialItemModel { Name = "To Be Deleted" };
        await _context.SpecialItems.AddAsync(specialItem);
        await _context.SaveChangesAsync();
        var id = specialItem.Id;

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully deleted"
        };
        A.CallTo(() => _operationResultFactory.SuccessDeleted(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteSpecialPositionAsync(id);

        // Assert
        result.Should().Be(expectedResult);
        var deletedItem = await _context.SpecialItems.FindAsync(id);
        deletedItem.Should().BeNull();
    }

    [Test]
    public async Task DeleteSpecialPositionAsync_WhenSpecialItemDoesNotExist_ShouldReturnNotFound()
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
        var result = await _sut.DeleteSpecialPositionAsync(999);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _operationResultFactory.NotFound(A<string>._, 999))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteSpecialPositionAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to delete"
        };
        A.CallTo(() => _operationResultFactory.FailedToDelete(A<string>._, A<string?>._))
            .Returns(expectedResult);

        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new SpecialItemService(disposedContext, _logger, _localizer, _operationResultFactory);

        // Act
        var result = await _sut.DeleteSpecialPositionAsync(1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion
}