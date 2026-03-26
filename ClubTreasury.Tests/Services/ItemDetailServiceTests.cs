using FakeItEasy;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ClubTreasury.Data;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.ItemDetail;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Tests.Services;

[TestFixture]
public class ItemDetailServiceTests
{
    private CashDataContext _context = null!;
    private ILogger<ItemDetailService> _logger = null!;
    private IResultFactory _resultFactory = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private ItemDetailService _sut = null!;
    private bool _contextDisposed;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CashDataContext(options);
        _contextDisposed = false;
        _logger = A.Fake<ILogger<ItemDetailService>>();
        _resultFactory = A.Fake<IResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();

        A.CallTo(() => _localizer["ItemDetail"])
            .Returns(new LocalizedString("ItemDetail", "Item Detail"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));

        _sut = new ItemDetailService(_context, _logger, _localizer, _resultFactory);
    }

    [TearDown]
    public void TearDown()
    {
        if (_contextDisposed) return;

        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetAllItemDetailsAsync Tests

    [Test]
    public async Task GetAllItemDetailsAsync_WhenNoItemDetailsExist_ShouldReturnEmptyList()
    {
        // Act
        var result = await _sut.GetAllItemDetailsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllItemDetailsAsync_WhenItemDetailsExist_ShouldReturnAllOrderedByIdDescending()
    {
        // Arrange
        var itemDetails = new List<ItemDetailModel>
        {
            new() { CostDetails = "First Item" },
            new() { CostDetails = "Second Item" },
            new() { CostDetails = "Third Item" }
        };
        await _context.ItemDetails.AddRangeAsync(itemDetails);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllItemDetailsAsync();

        // Assert
        result.Should().HaveCount(3);
        result.First().CostDetails.Should().Be("Third Item");
    }

    #endregion

    #region GetItemDetailByIdAsync Tests

    [Test]
    public async Task GetItemDetailByIdAsync_WhenItemDetailExists_ShouldReturnItemDetail()
    {
        // Arrange
        var itemDetail = new ItemDetailModel { CostDetails = "Test Item" };
        await _context.ItemDetails.AddAsync(itemDetail);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetItemDetailByIdAsync(itemDetail.Id);

        // Assert
        result.Should().NotBeNull();
        result.CostDetails.Should().Be("Test Item");
    }

    [Test]
    public async Task GetItemDetailByIdAsync_WhenItemDetailDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetItemDetailByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetItemDetailByNameAsync Tests

    [Test]
    public async Task GetItemDetailByNameAsync_WhenItemDetailExists_ShouldReturnItemDetail()
    {
        // Arrange
        var itemDetail = new ItemDetailModel { CostDetails = "Specific Item" };
        await _context.ItemDetails.AddAsync(itemDetail);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetItemDetailByNameAsync("Specific Item");

        // Assert
        result.Should().NotBeNull();
        result.CostDetails.Should().Be("Specific Item");
    }

    [Test]
    public async Task GetItemDetailByNameAsync_WhenItemDetailDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetItemDetailByNameAsync("NonExistent");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetItemDetailByCategoryIdAsync Tests

    [Test]
    public async Task GetItemDetailByCategoryIdAsync_WhenItemDetailsExist_ShouldReturnItemDetails()
    {
        // Arrange
        var costCenter = new CostCenterModel { CostUnitName = "Test Center" };
        await _context.CostCenters.AddAsync(costCenter);

        var category = new CategoryModel { Name = "Test Category" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var itemDetails = new List<ItemDetailModel>
        {
            new() { CostDetails = "Item 1" },
            new() { CostDetails = "Item 2" }
        };
        await _context.ItemDetails.AddRangeAsync(itemDetails);
        await _context.SaveChangesAsync();

        var allocations = itemDetails.Select(i => new AllocationModel
        {
            CostCenterId = costCenter.Id,
            CategoryId = category.Id,
            ItemDetailId = i.Id
        }).ToList();
        await _context.Allocations.AddRangeAsync(allocations);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetItemDetailByCategoryIdAsync(category.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Select(i => i.CostDetails).Should().BeEquivalentTo(["Item 1", "Item 2"]);
    }

    [Test]
    public async Task GetItemDetailByCategoryIdAsync_WhenNoItemDetailsForCategory_ShouldReturnEmptyList()
    {
        // Arrange
        var category = new CategoryModel { Name = "Empty Category" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetItemDetailByCategoryIdAsync(category.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetItemDetailByCategoryIdAsync_WhenCategoryDoesNotExist_ShouldReturnEmptyList()
    {
        // Act
        var result = await _sut.GetItemDetailByCategoryIdAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddItemDetailAsync Tests

    [Test]
    public async Task AddItemDetailAsync_WhenValidItemDetail_ShouldAddAndReturnSuccess()
    {
        // Arrange
        var itemDetail = new ItemDetailModel { CostDetails = "New Item" };
        var expectedResult = Result.Success("Successfully added");
        A.CallTo(() => _resultFactory.SuccessAdded(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddItemDetailAsync(itemDetail);

        // Assert
        result.Should().Be(expectedResult);
        var addedItem = await _context.ItemDetails.FirstOrDefaultAsync(i => i.CostDetails == "New Item");
        addedItem.Should().NotBeNull();
        A.CallTo(() => _resultFactory.SuccessAdded(
            A<string>.That.Contains("New Item"),
            A<object?>._)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task AddItemDetailAsync_WhenNameAlreadyExists_ShouldReturnAlreadyExists()
    {
        // Arrange
        var existing = new ItemDetailModel { CostDetails = "Hallennutzung" };
        await _context.ItemDetails.AddAsync(existing);
        await _context.SaveChangesAsync();

        var duplicate = new ItemDetailModel { CostDetails = "Hallennutzung" };
        var expectedResult = Result.Failure(new Error("Entity.AlreadyExists", "Already exists"));
        A.CallTo(() => _resultFactory.AlreadyExists(A<string>._, A<string?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddItemDetailAsync(duplicate);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _resultFactory.AlreadyExists(A<string>._, A<string>.That.Contains("Hallennutzung")))
            .MustHaveHappenedOnceExactly();
        var count = await _context.ItemDetails.CountAsync(i => i.CostDetails == "Hallennutzung");
        count.Should().Be(1);
    }

    [Test]
    public async Task AddItemDetailAsync_WhenExceptionOccurs_ShouldReturnFailure()
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

        _sut = new ItemDetailService(disposedContext, _logger, _localizer, _resultFactory);

        var itemDetail = new ItemDetailModel { CostDetails = "New Item" };

        // Act
        var result = await _sut.AddItemDetailAsync(itemDetail);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region UpdateItemDetailAsync Tests

    [Test]
    public async Task UpdateItemDetailAsync_WhenValidItemDetail_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var itemDetail = new ItemDetailModel { CostDetails = "Original Name" };
        await _context.ItemDetails.AddAsync(itemDetail);
        await _context.SaveChangesAsync();

        var expectedResult = Result.Success("Successfully updated");
        A.CallTo(() => _resultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);

        itemDetail.CostDetails = "Updated Name";

        // Act
        var result = await _sut.UpdateItemDetailAsync(itemDetail);

        // Assert
        result.Should().Be(expectedResult);
        var updatedItem = await _context.ItemDetails.FindAsync(itemDetail.Id);
        updatedItem!.CostDetails.Should().Be("Updated Name");
    }

    [Test]
    public async Task UpdateItemDetailAsync_WhenExceptionOccurs_ShouldReturnFailure()
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

        _sut = new ItemDetailService(disposedContext, _logger, _localizer, _resultFactory);

        var itemDetail = new ItemDetailModel { CostDetails = "Test" };

        // Act
        var result = await _sut.UpdateItemDetailAsync(itemDetail);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region DeleteItemDetailAsync Tests

    [Test]
    public async Task DeleteItemDetailAsync_WhenItemDetailExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var itemDetail = new ItemDetailModel { CostDetails = "To Be Deleted" };
        await _context.ItemDetails.AddAsync(itemDetail);
        await _context.SaveChangesAsync();
        var id = itemDetail.Id;

        var expectedResult = Result.Success("Successfully deleted");
        A.CallTo(() => _resultFactory.SuccessDeleted(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteItemDetailAsync(id);

        // Assert
        result.Should().Be(expectedResult);
        var deletedItem = await _context.ItemDetails.FindAsync(id);
        deletedItem.Should().BeNull();
    }

    [Test]
    public async Task DeleteItemDetailAsync_WhenItemDetailDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Not found"));
        A.CallTo(() => _resultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteItemDetailAsync(999);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _resultFactory.NotFound(A<string>._, 999))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteItemDetailAsync_WhenExceptionOccurs_ShouldReturnFailure()
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

        _sut = new ItemDetailService(disposedContext, _logger, _localizer, _resultFactory);

        // Act
        var result = await _sut.DeleteItemDetailAsync(1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion
    
}