using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TTCCashRegister.Data;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;
using TTCCashRegister.Data.ItemDetail;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Tests.Services;

[TestFixture]
public class AllocationServiceTests
{
    private CashDataContext _context = null!;
    private ILogger<AllocationService> _logger = null!;
    private IOperationResultFactory _operationResultFactory = null!;
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
        _operationResultFactory = A.Fake<IOperationResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();
        _costCenterService = A.Fake<ICostCenterService>();
        _categoryService = A.Fake<ICategoryService>();
        _itemDetailService = A.Fake<IItemDetailService>();

        A.CallTo(() => _localizer["Allocation"])
            .Returns(new LocalizedString("Allocation", "Allocation"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));

        _sut = new AllocationService(
            _context,
            _logger,
            _operationResultFactory,
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
        result!.CostCenter.CostUnitName.Should().Be("Test Cost Center");
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

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully added"
        };
        A.CallTo(() => _operationResultFactory.SuccessAdded(A<string>._, A<object?>._))
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

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Already exists"
        };
        A.CallTo(() => _operationResultFactory.AlreadyExists(A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddAllocationAsync(newAllocation);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _operationResultFactory.AlreadyExists(A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task AddAllocationAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to add"
        };
        A.CallTo(() => _operationResultFactory.FailedToAdd(A<string>._, A<string?>._))
            .Returns(expectedResult);

        _context.Dispose();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        disposedContext.Dispose();

        _sut = new AllocationService(
            disposedContext, _logger, _operationResultFactory, _localizer,
            _costCenterService, _categoryService, _itemDetailService);

        var allocation = new AllocationModel { CostCenterId = 1, CategoryId = 1 };

        // Act
        var result = await _sut.AddAllocationAsync(allocation);

        // Assert
        result.Should().Be(expectedResult);
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

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully updated"
        };
        A.CallTo(() => _operationResultFactory.SuccessUpdated(A<string>._, A<object?>._))
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

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Not found"
        };
        A.CallTo(() => _operationResultFactory.NotFound(A<string>._, A<object>._))
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
        A.CallTo(() => _operationResultFactory.NotFound(A<string>._, 999))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task UpdateAllocationAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to update"
        };
        A.CallTo(() => _operationResultFactory.FailedToUpdate(A<string>._, A<string?>._))
            .Returns(expectedResult);

        _context.Dispose();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        disposedContext.Dispose();

        _sut = new AllocationService(
            disposedContext, _logger, _operationResultFactory, _localizer,
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

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully deleted"
        };
        A.CallTo(() => _operationResultFactory.SuccessDeleted(A<string>._, A<object?>._))
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
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Not found"
        };
        A.CallTo(() => _operationResultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteAllocationAsync(999);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _operationResultFactory.NotFound(A<string>._, 999))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteAllocationAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to delete"
        };
        A.CallTo(() => _operationResultFactory.FailedToDelete(A<string>._, A<string?>._))
            .Returns(expectedResult);

        _context.Dispose();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        disposedContext.Dispose();

        _sut = new AllocationService(
            disposedContext, _logger, _operationResultFactory, _localizer,
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
        var newCostCenter = new CostCenterModel { Id = 1, CostUnitName = "New Cost Center" };
        var category = new CategoryModel { Id = 1, Name = "Existing Category" };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        A.CallTo(() => _costCenterService.GetCostCenterByNameAsync("New Cost Center"))
            .Returns((CostCenterModel?)null);
        A.CallTo(() => _costCenterService.AddCostCenterAsync(A<CostCenterModel>._))
            .Invokes((CostCenterModel cc) =>
            {
                cc.Id = 1;
                _context.CostCenters.Add(cc);
                _context.SaveChanges();
            })
            .Returns(new OperationResult { Status = OperationResultStatus.Success });
        A.CallTo(() => _categoryService.GetCategoryByNameAsync("Existing Category"))
            .Returns(category);

        // Act
        var result = await _sut.GetOrCreateAllocationAsync("New Cost Center", "Existing Category");

        // Assert
        result.Should().NotBeNull();
        A.CallTo(() => _costCenterService.AddCostCenterAsync(A<CostCenterModel>.That.Matches(c => c.CostUnitName == "New Cost Center")))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task GetOrCreateAllocationAsync_WhenCategoryDoesNotExist_ShouldCreateCategory()
    {
        // Arrange
        var costCenter = new CostCenterModel { Id = 1, CostUnitName = "Existing Cost Center" };

        await _context.CostCenters.AddAsync(costCenter);
        await _context.SaveChangesAsync();

        A.CallTo(() => _costCenterService.GetCostCenterByNameAsync("Existing Cost Center"))
            .Returns(costCenter);
        A.CallTo(() => _categoryService.GetCategoryByNameAsync("New Category"))
            .Returns((CategoryModel?)null);
        A.CallTo(() => _categoryService.AddCategoryAsync(A<CategoryModel>._))
            .Invokes((CategoryModel cat) =>
            {
                cat.Id = 1;
                _context.Categories.Add(cat);
                _context.SaveChanges();
            })
            .Returns(new OperationResult { Status = OperationResultStatus.Success });

        // Act
        var result = await _sut.GetOrCreateAllocationAsync("Existing Cost Center", "New Category");

        // Assert
        result.Should().NotBeNull();
        A.CallTo(() => _categoryService.AddCategoryAsync(A<CategoryModel>.That.Matches(c => c.Name == "New Category")))
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

        A.CallTo(() => _costCenterService.GetCostCenterByNameAsync("Cost Center"))
            .Returns(costCenter);
        A.CallTo(() => _categoryService.GetCategoryByNameAsync("Category"))
            .Returns(category);
        A.CallTo(() => _itemDetailService.GetItemDetailByNameAsync("New Item Detail"))
            .Returns((ItemDetailModel?)null);
        A.CallTo(() => _itemDetailService.AddItemDetailAsync(A<ItemDetailModel>._))
            .Invokes((ItemDetailModel item) =>
            {
                item.Id = 1;
                _context.ItemDetails.Add(item);
                _context.SaveChanges();
            })
            .Returns(new OperationResult { Status = OperationResultStatus.Success });

        // Act
        var result = await _sut.GetOrCreateAllocationAsync("Cost Center", "Category", "New Item Detail");

        // Assert
        result.Should().NotBeNull();
        A.CallTo(() => _itemDetailService.AddItemDetailAsync(A<ItemDetailModel>.That.Matches(i => i.CostDetails == "New Item Detail")))
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

        A.CallTo(() => _costCenterService.GetCostCenterByNameAsync("Cost Center"))
            .Returns(costCenter);
        A.CallTo(() => _categoryService.GetCategoryByNameAsync("Category"))
            .Returns(category);

        // Act
        var result = await _sut.GetOrCreateAllocationAsync("Cost Center", "Category", null);

        // Assert
        result.Should().NotBeNull();
        result.ItemDetailId.Should().BeNull();
        A.CallTo(() => _itemDetailService.GetItemDetailByNameAsync(A<string>._))
            .MustNotHaveHappened();
    }

    #endregion
}