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
public class AllocationServiceTests
{
    private CashDataContext _context = null!;
    private ILogger<AllocationService> _logger = null!;
    private IResultFactory _resultFactory = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private ICostCenterService _costCenterService = null!;
    private ICategoryService _categoryService = null!;
    private IItemDetailService _itemDetailService = null!;
    private AllocationService _sut = null!;
    private bool _contextDisposed;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CashDataContext(options);
        _contextDisposed = false;
        _logger = A.Fake<ILogger<AllocationService>>();
        _resultFactory = A.Fake<IResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();
        _costCenterService = A.Fake<ICostCenterService>();
        _categoryService = A.Fake<ICategoryService>();
        _itemDetailService = A.Fake<IItemDetailService>();

        A.CallTo(() => _localizer["Allocation"])
            .Returns(new LocalizedString("Allocation", "Allocation"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));
        A.CallTo(() => _localizer["CostCenter"])
            .Returns(new LocalizedString("CostCenter", "Cost Center"));
        A.CallTo(() => _localizer["Category"])
            .Returns(new LocalizedString("Category", "Category"));

        _sut = new AllocationService(
            _context,
            _logger,
            _resultFactory,
            _localizer,
            _costCenterService,
            _categoryService,
            _itemDetailService);
    }

    [TearDown]
    public void TearDown()
    {
        if (_contextDisposed) return;

        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<(CostCenterModel CostCenter, CategoryModel Category)> CreateTestDataAsync()
    {
        var costCenter = new CostCenterModel { CostUnitName = "Test Cost Center" };
        var category = new CategoryModel { Name = "Test Category" };

        await _context.CostCenters.AddAsync(costCenter);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        return (costCenter, category);
    }

    #region GetAllAllocationsAsync Tests

    [Test]
    public async Task GetAllAllocationsAsync_WhenNoAllocationsExist_ShouldReturnEmptyList()
    {
        // Act
        var result = await _sut.GetAllAllocationsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllAllocationsAsync_WhenAllocationsExist_ShouldReturnAllAllocations()
    {
        // Arrange
        var (costCenter, category) = await CreateTestDataAsync();

        var allocations = new List<AllocationModel>
        {
            new() { CostCenterId = costCenter.Id, CategoryId = category.Id },
        };
        await _context.Allocations.AddRangeAsync(allocations);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAllocationsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().CostCenter.Should().NotBeNull();
        result.First().Category.Should().NotBeNull();
    }

    #endregion

    #region GetAllocationsByIdAsync Tests

    [Test]
    public async Task GetAllocationsByIdAsync_WhenAllocationExists_ShouldReturnAllocation()
    {
        // Arrange
        var (costCenter, category) = await CreateTestDataAsync();
        var allocation = new AllocationModel { CostCenterId = costCenter.Id, CategoryId = category.Id };
        await _context.Allocations.AddAsync(allocation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllocationsByIdAsync(allocation.Id);

        // Assert
        result.Should().NotBeNull();
        result.CostCenter.CostUnitName.Should().Be("Test Cost Center");
        result.Category.Name.Should().Be("Test Category");
    }

    [Test]
    public async Task GetAllocationsByIdAsync_WhenAllocationDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetAllocationsByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion
    
    #region AddAllocationAsync Tests

    [Test]
    public async Task AddAllocationAsync_WhenValidAllocation_ShouldAddAndReturnSuccess()
    {
        // Arrange
        var (costCenter, category) = await CreateTestDataAsync();
        var allocation = new AllocationModel { CostCenterId = costCenter.Id, CategoryId = category.Id };

        var expectedResult = Result.Success("Successfully added");
        A.CallTo(() => _resultFactory.SuccessAdded(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddAllocationAsync(allocation);

        // Assert
        result.Should().Be(expectedResult);
        var addedAllocation = await _context.Allocations.FirstOrDefaultAsync();
        addedAllocation.Should().NotBeNull();
    }

    [Test]
    public async Task AddAllocationAsync_WhenAllocationAlreadyExists_ShouldReturnAlreadyExists()
    {
        // Arrange
        var (costCenter, category) = await CreateTestDataAsync();
        var existingAllocation = new AllocationModel { CostCenterId = costCenter.Id, CategoryId = category.Id };
        await _context.Allocations.AddAsync(existingAllocation);
        await _context.SaveChangesAsync();

        var newAllocation = new AllocationModel { CostCenterId = costCenter.Id, CategoryId = category.Id };

        var expectedResult = Result.Failure(new Error("Test.Error", "Already exists"));
        A.CallTo(() => _resultFactory.AlreadyExists(A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddAllocationAsync(newAllocation);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _resultFactory.AlreadyExists(A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task AddAllocationAsync_WhenExceptionOccurs_ShouldReturnFailure()
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

        _sut = new AllocationService(
            disposedContext, _logger, _resultFactory, _localizer,
            _costCenterService, _categoryService, _itemDetailService);

        var allocation = new AllocationModel { CostCenterId = 1, CategoryId = 1 };

        // Act
        var result = await _sut.AddAllocationAsync(allocation);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task AddAllocationAsync_WhenCostCenterIdIsZero_ShouldReturnDialogIsEmpty()
    {
        // Arrange
        var (_, category) = await CreateTestDataAsync();
        var allocation = new AllocationModel { CostCenterId = 0, CategoryId = category.Id };

        var expectedResult = Result.Failure(new Error("Test.Error", "Dialog is empty"));
        A.CallTo(() => _resultFactory.DialogIsEmpty(A<string>._, A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddAllocationAsync(allocation);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _resultFactory.DialogIsEmpty(A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
        var addedAllocation = await _context.Allocations.FirstOrDefaultAsync();
        addedAllocation.Should().BeNull();
    }

    [Test]
    public async Task AddAllocationAsync_WhenCategoryIdIsZero_ShouldReturnDialogIsEmpty()
    {
        // Arrange
        var (costCenter, _) = await CreateTestDataAsync();
        var allocation = new AllocationModel { CostCenterId = costCenter.Id, CategoryId = 0 };

        var expectedResult = Result.Failure(new Error("Test.Error", "Dialog is empty"));
        A.CallTo(() => _resultFactory.DialogIsEmpty(A<string>._, A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddAllocationAsync(allocation);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _resultFactory.DialogIsEmpty(A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
        var addedAllocation = await _context.Allocations.FirstOrDefaultAsync();
        addedAllocation.Should().BeNull();
    }

    #endregion

    #region UpdateAllocationAsync Tests

    [Test]
    public async Task UpdateAllocationAsync_WhenAllocationExists_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var (costCenter, category) = await CreateTestDataAsync();
        var newCategory = new CategoryModel { Name = "New Category" };
        await _context.Categories.AddAsync(newCategory);
        await _context.SaveChangesAsync();

        var allocation = new AllocationModel { CostCenterId = costCenter.Id, CategoryId = category.Id };
        await _context.Allocations.AddAsync(allocation);
        await _context.SaveChangesAsync();

        var expectedResult = Result.Success("Successfully updated");
        A.CallTo(() => _resultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);

        var updatedAllocation = new AllocationModel
        {
            Id = allocation.Id,
            CostCenterId = costCenter.Id,
            CategoryId = newCategory.Id,
            CostCenter = costCenter,
            Category = newCategory
        };

        // Act
        var result = await _sut.UpdateAllocationAsync(updatedAllocation);

        // Assert
        result.Should().Be(expectedResult);
        var updated = await _context.Allocations.FindAsync(allocation.Id);
        updated!.CategoryId.Should().Be(newCategory.Id);
    }

    [Test]
    public async Task UpdateAllocationAsync_WhenAllocationDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var (costCenter, category) = await CreateTestDataAsync();

        var expectedResult = Result.Failure(new Error("Test.Error", "Not found"));
        A.CallTo(() => _resultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        var allocation = new AllocationModel
        {
            Id = 999,
            CostCenterId = costCenter.Id,
            CategoryId = category.Id,
            CostCenter = costCenter,
            Category = category
        };

        // Act
        var result = await _sut.UpdateAllocationAsync(allocation);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _resultFactory.NotFound(A<string>._, 999))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task UpdateAllocationAsync_WhenExceptionOccurs_ShouldReturnFailure()
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

        _sut = new AllocationService(
            disposedContext, _logger, _resultFactory, _localizer,
            _costCenterService, _categoryService, _itemDetailService);

        var allocation = new AllocationModel
        {
            Id = 1,
            CostCenterId = 1,
            CategoryId = 1,
            CostCenter = new CostCenterModel { CostUnitName = "Test" },
            Category = new CategoryModel { Name = "Test" }
        };

        // Act
        var result = await _sut.UpdateAllocationAsync(allocation);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region DeleteAllocationAsync Tests

    [Test]
    public async Task DeleteAllocationAsync_WhenAllocationExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var (costCenter, category) = await CreateTestDataAsync();
        var allocation = new AllocationModel { CostCenterId = costCenter.Id, CategoryId = category.Id };
        await _context.Allocations.AddAsync(allocation);
        await _context.SaveChangesAsync();
        var id = allocation.Id;

        var expectedResult = Result.Success("Successfully deleted");
        A.CallTo(() => _resultFactory.SuccessDeleted(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteAllocationAsync(id);

        // Assert
        result.Should().Be(expectedResult);
        var deletedAllocation = await _context.Allocations.FindAsync(id);
        deletedAllocation.Should().BeNull();
    }

    [Test]
    public async Task DeleteAllocationAsync_WhenAllocationDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Not found"));
        A.CallTo(() => _resultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteAllocationAsync(999);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _resultFactory.NotFound(A<string>._, 999))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteAllocationAsync_WhenExceptionOccurs_ShouldReturnFailure()
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

        _sut = new AllocationService(
            disposedContext, _logger, _resultFactory, _localizer,
            _costCenterService, _categoryService, _itemDetailService);

        // Act
        var result = await _sut.DeleteAllocationAsync(1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region GetRequiredAllocationAsync Tests

    [Test]
    public async Task GetRequiredAllocationAsync_WhenAllocationExists_ShouldReturnAllocation()
    {
        // Arrange
        var (costCenter, category) = await CreateTestDataAsync();
        var allocation = new AllocationModel { CostCenterId = costCenter.Id, CategoryId = category.Id };
        await _context.Allocations.AddAsync(allocation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetRequiredAllocationAsync(allocation.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(allocation.Id);
    }

    [Test]
    public async Task GetRequiredAllocationAsync_WhenAllocationDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Act
        var act = async () => await _sut.GetRequiredAllocationAsync(999);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Allocation 999 not found.");
    }

    #endregion

    #region GetOrCreateAllocationAsync Tests

    [Test]
    public async Task GetOrCreateAllocationAsync_WhenAllEntitiesExist_ShouldReturnExistingAllocation()
    {
        // Arrange
        var costCenter = new CostCenterModel { Id = 1, CostUnitName = "Existing Cost Center" };
        var category = new CategoryModel { Id = 1, Name = "Existing Category" };

        await _context.CostCenters.AddAsync(costCenter);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var existingAllocation = new AllocationModel
        {
            CostCenterId = costCenter.Id,
            CategoryId = category.Id
        };
        await _context.Allocations.AddAsync(existingAllocation);
        await _context.SaveChangesAsync();

        A.CallTo(() => _costCenterService.GetCostCenterByNameAsync("Existing Cost Center"))
            .Returns(costCenter);
        A.CallTo(() => _categoryService.GetCategoryByNameAsync("Existing Category"))
            .Returns(category);

        // Act
        var result = await _sut.GetOrCreateAllocationAsync("Existing Cost Center", "Existing Category");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingAllocation.Id);
    }

    [Test]
    public async Task GetOrCreateAllocationAsync_WhenCostCenterDoesNotExist_ShouldCreateCostCenter()
    {
        // Arrange
        var category = new CategoryModel { Id = 1, Name = "Existing Category" };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        A.CallTo(() => _costCenterService.GetCostCenterByNameAsync("New Cost Center", A<CancellationToken>._))
            .Returns((CostCenterModel?)null);
        A.CallTo(() => _costCenterService.AddCostCenterAsync(A<CostCenterModel>._, A<CancellationToken>._))
            .Invokes((CostCenterModel cc, CancellationToken _) =>
            {
                cc.Id = 1;
                _context.CostCenters.Add(cc);
                _context.SaveChanges();
            })
            .Returns(Result.Success());
        A.CallTo(() => _categoryService.GetCategoryByNameAsync("Existing Category", A<CancellationToken>._))
            .Returns(category);

        // Act
        var result = await _sut.GetOrCreateAllocationAsync("New Cost Center", "Existing Category");

        // Assert
        result.Should().NotBeNull();
        A.CallTo(() => _costCenterService.AddCostCenterAsync(A<CostCenterModel>.That.Matches(c => c.CostUnitName == "New Cost Center"), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task GetOrCreateAllocationAsync_WhenCategoryDoesNotExist_ShouldCreateCategory()
    {
        // Arrange
        var costCenter = new CostCenterModel { Id = 1, CostUnitName = "Existing Cost Center" };

        await _context.CostCenters.AddAsync(costCenter);
        await _context.SaveChangesAsync();

        A.CallTo(() => _costCenterService.GetCostCenterByNameAsync("Existing Cost Center", A<CancellationToken>._))
            .Returns(costCenter);
        A.CallTo(() => _categoryService.GetCategoryByNameAsync("New Category", A<CancellationToken>._))
            .Returns((CategoryModel?)null);
        A.CallTo(() => _categoryService.AddCategoryAsync(A<CategoryModel>._, A<CancellationToken>._))
            .Invokes((CategoryModel cat, CancellationToken _) =>
            {
                cat.Id = 1;
                _context.Categories.Add(cat);
                _context.SaveChanges();
            })
            .Returns(Result.Success());

        // Act
        var result = await _sut.GetOrCreateAllocationAsync("Existing Cost Center", "New Category");

        // Assert
        result.Should().NotBeNull();
        A.CallTo(() => _categoryService.AddCategoryAsync(A<CategoryModel>.That.Matches(c => c.Name == "New Category"), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task GetOrCreateAllocationAsync_WithItemDetail_WhenItemDetailDoesNotExist_ShouldCreateItemDetail()
    {
        // Arrange
        var costCenter = new CostCenterModel { Id = 1, CostUnitName = "Cost Center" };
        var category = new CategoryModel { Id = 1, Name = "Category" };

        await _context.CostCenters.AddAsync(costCenter);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        A.CallTo(() => _costCenterService.GetCostCenterByNameAsync("Cost Center", A<CancellationToken>._))
            .Returns(costCenter);
        A.CallTo(() => _categoryService.GetCategoryByNameAsync("Category", A<CancellationToken>._))
            .Returns(category);
        A.CallTo(() => _itemDetailService.GetItemDetailByNameAsync("New Item Detail", A<CancellationToken>._))
            .Returns((ItemDetailModel?)null);
        A.CallTo(() => _itemDetailService.AddItemDetailAsync(A<ItemDetailModel>._, A<CancellationToken>._))
            .Invokes((ItemDetailModel item, CancellationToken _) =>
            {
                item.Id = 1;
                _context.ItemDetails.Add(item);
                _context.SaveChanges();
            })
            .Returns(Result.Success());

        // Act
        var result = await _sut.GetOrCreateAllocationAsync("Cost Center", "Category", "New Item Detail");

        // Assert
        result.Should().NotBeNull();
        A.CallTo(() => _itemDetailService.AddItemDetailAsync(A<ItemDetailModel>.That.Matches(i => i.CostDetails == "New Item Detail"), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task GetOrCreateAllocationAsync_WithNullItemDetail_ShouldNotCreateItemDetail()
    {
        // Arrange
        var costCenter = new CostCenterModel { Id = 1, CostUnitName = "Cost Center" };
        var category = new CategoryModel { Id = 1, Name = "Category" };

        await _context.CostCenters.AddAsync(costCenter);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        A.CallTo(() => _costCenterService.GetCostCenterByNameAsync("Cost Center", A<CancellationToken>._))
            .Returns(costCenter);
        A.CallTo(() => _categoryService.GetCategoryByNameAsync("Category", A<CancellationToken>._))
            .Returns(category);

        // Act
        var result = await _sut.GetOrCreateAllocationAsync("Cost Center", "Category");

        // Assert
        result.Should().NotBeNull();
        result.ItemDetailId.Should().BeNull();
        A.CallTo(() => _itemDetailService.GetItemDetailByNameAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    #endregion
}