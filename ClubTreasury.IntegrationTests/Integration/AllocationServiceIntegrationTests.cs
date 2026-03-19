using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.IntegrationTests.Integration;

[TestFixture]
public class AllocationServiceIntegrationTests : IntegrationTestBase
{
    private IAllocationService _allocationService = null!;

    [SetUp]
    public new async Task SetUp()
    {
        await base.SetUp();
        _allocationService = GetService<IAllocationService>();
    }

    [Test]
    public async Task GetOrCreateAllocation_WhenNothingExists_ShouldCreateCostCenterCategoryAndAllocation()
    {
        // Act
        var allocation = await _allocationService.GetOrCreateAllocationAsync(
            "New Cost Center", "New Category");

        // Assert
        allocation.Should().NotBeNull();
        allocation.Id.Should().BeGreaterThan(0);

        var savedCostCenter = await GetDbContext().CostCenters
            .FirstOrDefaultAsync(c => c.CostUnitName == "New Cost Center");
        savedCostCenter.Should().NotBeNull();

        var savedCategory = await GetDbContext().Categories
            .FirstOrDefaultAsync(c => c.Name == "New Category");
        savedCategory.Should().NotBeNull();

        allocation.CostCenterId.Should().Be(savedCostCenter!.Id);
        allocation.CategoryId.Should().Be(savedCategory!.Id);
    }

    [Test]
    public async Task GetOrCreateAllocation_WhenCalledTwiceWithSameNames_ShouldReturnSameAllocation()
    {
        // Act
        var firstAllocation = await _allocationService.GetOrCreateAllocationAsync(
            "Reused Cost Center", "Reused Category");
        var secondAllocation = await _allocationService.GetOrCreateAllocationAsync(
            "Reused Cost Center", "Reused Category");

        // Assert
        firstAllocation.Id.Should().Be(secondAllocation.Id);

        var costCenterCount = await GetDbContext().CostCenters
            .CountAsync(c => c.CostUnitName == "Reused Cost Center");
        costCenterCount.Should().Be(1);

        var categoryCount = await GetDbContext().Categories
            .CountAsync(c => c.Name == "Reused Category");
        categoryCount.Should().Be(1);
    }

    [Test]
    public async Task GetOrCreateAllocation_WithItemDetail_ShouldCreateAllEntities()
    {
        // Act
        var allocation = await _allocationService.GetOrCreateAllocationAsync(
            "Cost Center With Detail", "Category With Detail", "Some Item Detail");

        // Assert
        allocation.Should().NotBeNull();
        allocation.ItemDetailId.Should().NotBeNull();

        var savedItemDetail = await GetDbContext().ItemDetails
            .FirstOrDefaultAsync(i => i.CostDetails == "Some Item Detail");
        savedItemDetail.Should().NotBeNull();
        allocation.ItemDetailId.Should().Be(savedItemDetail!.Id);
    }

    [Test]
    public async Task AddAllocation_WithDuplicateCombination_ShouldReturnAlreadyExists()
    {
        // Arrange
        var costCenter = new CostCenterModel { CostUnitName = "Duplicate Test CC" };
        var category = new CategoryModel { Name = "Duplicate Test Cat" };
        await GetDbContext().CostCenters.AddAsync(costCenter);
        await GetDbContext().Categories.AddAsync(category);
        await GetDbContext().SaveChangesAsync();

        var firstAllocation = new AllocationModel
        {
            CostCenterId = costCenter.Id,
            CategoryId = category.Id
        };
        var firstResult = await _allocationService.AddAllocationAsync(firstAllocation);
        firstResult.IsSuccess.Should().BeTrue();

        // Act
        var duplicateAllocation = new AllocationModel
        {
            CostCenterId = costCenter.Id,
            CategoryId = category.Id
        };
        var duplicateResult = await _allocationService.AddAllocationAsync(duplicateAllocation);

        // Assert
        duplicateResult.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task DeleteAllocation_WhenReferencedByTransaction_ShouldFail()
    {
        // Arrange
        var costCenter = new CostCenterModel { CostUnitName = "Referenced CC" };
        var category = new CategoryModel { Name = "Referenced Cat" };
        var cashRegister = new CashRegisterModel { Name = "Test Register" };
        await GetDbContext().CostCenters.AddAsync(costCenter);
        await GetDbContext().Categories.AddAsync(category);
        await GetDbContext().CashRegisters.AddAsync(cashRegister);
        await GetDbContext().SaveChangesAsync();

        var allocation = new AllocationModel
        {
            CostCenterId = costCenter.Id,
            CategoryId = category.Id
        };
        await GetDbContext().Allocations.AddAsync(allocation);
        await GetDbContext().SaveChangesAsync();

        var transaction = new TransactionModel
        {
            Documentnumber = 9001,
            Description = "Blocking transaction",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id,
            Sum = 10.00m,
            AccountMovement = 10.00m,
            Date = DateOnly.FromDateTime(DateTime.Now)
        };
        await GetDbContext().Transactions.AddAsync(transaction);
        await GetDbContext().SaveChangesAsync();

        // Act
        var result = await _allocationService.DeleteAllocationAsync(allocation.Id);

        // Assert - should fail due to FK constraint
        result.IsFailure.Should().BeTrue();

        var stillExists = await GetDbContext().Allocations.FindAsync(allocation.Id);
        stillExists.Should().NotBeNull();
    }
}
