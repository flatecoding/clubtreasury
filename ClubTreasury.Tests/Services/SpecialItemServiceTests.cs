using FakeItEasy;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ClubTreasury.Data;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.SpecialItem;

namespace ClubTreasury.Tests.Services;

[TestFixture]
public class SpecialItemServiceTests
{
    private CashDataContext _context = null!;
    private ILogger<SpecialItemService> _logger = null!;
    private IResultFactory _resultFactory = null!;
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
        _resultFactory = A.Fake<IResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();

        A.CallTo(() => _localizer["SpecialPosition"])
            .Returns(new LocalizedString("SpecialPosition", "Special Position"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));

        _sut = new SpecialItemService(_context, _logger, _localizer, _resultFactory);
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
        var result = await _sut.GetAllSpecialItemsAsync();

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
        var result = await _sut.GetAllSpecialItemsAsync();

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
        var result = await _sut.GetSpecialPositionByIdAsync(specialItem.Id);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Special Item");
    }

    [Test]
    public async Task GetSpecialPositionById_WhenSpecialItemDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetSpecialPositionByIdAsync(999);

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
        var expectedResult = Result.Success("Successfully added");
        A.CallTo(() => _resultFactory.SuccessAdded(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddSpecialPositionAsync(specialItem);

        // Assert
        result.Should().Be(expectedResult);
        var addedItem = await _context.SpecialItems.FirstOrDefaultAsync(s => s.Name == "New Special Item");
        addedItem.Should().NotBeNull();
        A.CallTo(() => _resultFactory.SuccessAdded(
            A<string>.That.Contains("New Special Item"),
            A<object?>._)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task AddSpecialPositionAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Failed to add"));
        A.CallTo(() => _resultFactory.FailedToAdd(A<string>._, A<string?>._))
            .Returns(expectedResult);

        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new SpecialItemService(disposedContext, _logger, _localizer, _resultFactory);

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

        var expectedResult = Result.Success("Successfully updated");
        A.CallTo(() => _resultFactory.SuccessUpdated(A<string>._, A<object?>._))
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
        var expectedResult = Result.Failure(new Error("Test.Error", "Failed to update"));
        A.CallTo(() => _resultFactory.FailedToUpdate(A<string>._, A<string?>._))
            .Returns(expectedResult);

        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new SpecialItemService(disposedContext, _logger, _localizer, _resultFactory);

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

        var expectedResult = Result.Success("Successfully deleted");
        A.CallTo(() => _resultFactory.SuccessDeleted(A<string>._, A<object?>._))
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
        var expectedResult = Result.Failure(new Error("Test.Error", "Not found"));
        A.CallTo(() => _resultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteSpecialPositionAsync(999);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _resultFactory.NotFound(A<string>._, 999))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteSpecialPositionAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Failed to delete"));
        A.CallTo(() => _resultFactory.FailedToDelete(A<string>._, A<string?>._))
            .Returns(expectedResult);

        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new SpecialItemService(disposedContext, _logger, _localizer, _resultFactory);

        // Act
        var result = await _sut.DeleteSpecialPositionAsync(1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion
}