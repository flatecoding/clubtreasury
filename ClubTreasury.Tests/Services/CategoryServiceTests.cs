using FakeItEasy;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TTCCashRegister.Data;
using TTCCashRegister.Data.Allocation;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Tests.Services;

[TestFixture]
public class CategoryServiceTests
{
    private CashDataContext _context = null!;
    private ILogger<CategoryService> _logger = null!;
    private IOperationResultFactory _operationResultFactory = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private CategoryService _sut = null!;
    private bool _contextDisposed;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CashDataContext(options);
        _contextDisposed = false;
        _logger = A.Fake<ILogger<CategoryService>>();
        _operationResultFactory = A.Fake<IOperationResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();

        A.CallTo(() => _localizer["Category"])
            .Returns(new LocalizedString("Category", "Category"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));

        _sut = new CategoryService(_context, _logger, _operationResultFactory, _localizer);
    }

    [TearDown]
    public void TearDown()
    {
        if (_contextDisposed) return;

        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetAllCategoriesAsync Tests

    [Test]
    public async Task GetAllCategoriesAsync_WhenNoCategoriesExist_ShouldReturnEmptyList()
    {
        // Act
        var result = await _sut.GetAllCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllCategoriesAsync_WhenCategoriesExist_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<CategoryModel>
        {
            new() { Name = "Food" },
            new() { Name = "Drinks" },
            new() { Name = "Merchandise" }
        };
        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllCategoriesAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Select(c => c.Name).Should().BeEquivalentTo(["Food", "Drinks", "Merchandise"]);
    }

    #endregion

    #region GetCategoryByIdAsync Tests

    [Test]
    public async Task GetCategoryByIdAsync_WhenCategoryExists_ShouldReturnCategory()
    {
        // Arrange
        var category = new CategoryModel { Name = "Test Category" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCategoryByIdAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Category");
    }

    [Test]
    public async Task GetCategoryByIdAsync_WhenCategoryDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetCategoryByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetCategoryByNameAsync Tests

    [Test]
    public async Task GetCategoryByNameAsync_WhenCategoryExists_ShouldReturnCategory()
    {
        // Arrange
        var category = new CategoryModel { Name = "Food" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCategoryByNameAsync("Food");

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Food");
    }

    [Test]
    public async Task GetCategoryByNameAsync_WhenCategoryDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetCategoryByNameAsync("NonExistent");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetCategoriesByCostCenterIdAsync Tests

    [Test]
    public async Task GetCategoriesByCostCenterIdAsync_WhenCategoriesExist_ShouldReturnCategoriesOrderedByName()
    {
        // Arrange
        var costCenter = new CostCenterModel { CostUnitName = "Marketing" };
        await _context.CostCenters.AddAsync(costCenter);
        await _context.SaveChangesAsync();

        var categories = new List<CategoryModel>
        {
            new() { Name = "Zebra" },
            new() { Name = "Apple" },
            new() { Name = "Banana" }
        };
        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        var allocations = categories.Select(c => new AllocationModel
        {
            CostCenterId = costCenter.Id,
            CategoryId = c.Id
        }).ToList();
        await _context.Allocations.AddRangeAsync(allocations);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCategoriesByCostCenterIdAsync(costCenter.Id);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(3);
        resultList.Select(c => c.Name).Should().ContainInOrder("Apple", "Banana", "Zebra");
    }

    [Test]
    public async Task GetCategoriesByCostCenterIdAsync_WhenNoCategoriesForCostCenter_ShouldReturnEmptyList()
    {
        // Arrange
        var costCenter = new CostCenterModel { CostUnitName = "Empty Cost Center" };
        await _context.CostCenters.AddAsync(costCenter);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCategoriesByCostCenterIdAsync(costCenter.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetCategoriesByCostCenterIdAsync_WhenCostCenterDoesNotExist_ShouldReturnEmptyList()
    {
        // Act
        var result = await _sut.GetCategoriesByCostCenterIdAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddCategoryAsync Tests

    [Test]
    public async Task AddCategoryAsync_WhenValidCategory_ShouldAddAndReturnSuccess()
    {
        // Arrange
        var category = new CategoryModel { Name = "New Category" };
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully added"
        };
        A.CallTo(() => _operationResultFactory.SuccessAdded(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddCategoryAsync(category);

        // Assert
        result.Should().Be(expectedResult);
        var addedCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "New Category");
        addedCategory.Should().NotBeNull();
        A.CallTo(() => _operationResultFactory.SuccessAdded(
            A<string>.That.Contains("New Category"),
            A<object?>._)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task AddCategoryAsync_WhenExceptionOccurs_ShouldReturnFailure()
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

        _sut = new CategoryService(disposedContext, _logger, _operationResultFactory, _localizer);

        var category = new CategoryModel { Name = "New Category" };

        // Act
        var result = await _sut.AddCategoryAsync(category);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region UpdateCategoryAsync Tests

    [Test]
    public async Task UpdateCategoryAsync_WhenValidCategory_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var category = new CategoryModel { Name = "Original Name" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully updated"
        };
        A.CallTo(() => _operationResultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);

        category.Name = "Updated Name";

        // Act
        var result = await _sut.UpdateCategoryAsync(category);

        // Assert
        result.Should().Be(expectedResult);
        var updatedCategory = await _context.Categories.FindAsync(category.Id);
        updatedCategory!.Name.Should().Be("Updated Name");
    }

    [Test]
    public async Task UpdateCategoryAsync_WhenExceptionOccurs_ShouldReturnFailure()
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

        _sut = new CategoryService(disposedContext, _logger, _operationResultFactory, _localizer);

        var category = new CategoryModel { Name = "Test" };

        // Act
        var result = await _sut.UpdateCategoryAsync(category);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region DeleteCategoryAsync Tests

    [Test]
    public async Task DeleteCategoryAsync_WhenCategoryExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var category = new CategoryModel { Name = "To Be Deleted" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        var id = category.Id;

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully deleted"
        };
        A.CallTo(() => _operationResultFactory.SuccessDeleted(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteCategoryAsync(id);

        // Assert
        result.Should().Be(expectedResult);
        var deletedCategory = await _context.Categories.FindAsync(id);
        deletedCategory.Should().BeNull();
    }

    [Test]
    public async Task DeleteCategoryAsync_WhenCategoryDoesNotExist_ShouldReturnNotFound()
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
        var result = await _sut.DeleteCategoryAsync(999);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _operationResultFactory.NotFound(A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteCategoryAsync_WhenExceptionOccurs_ShouldReturnFailure()
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

        _sut = new CategoryService(disposedContext, _logger, _operationResultFactory, _localizer);

        // Act
        var result = await _sut.DeleteCategoryAsync(1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Constructor Tests

    [Test]
    public void Constructor_WhenContextIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new CategoryService(null!, _logger, _operationResultFactory, _localizer);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    #endregion
}