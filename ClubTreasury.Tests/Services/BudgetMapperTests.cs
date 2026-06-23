using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using ClubTreasury.Data.Mapper;
using ClubTreasury.Data.Mapper.DTOs;

namespace ClubTreasury.Tests.Services;

[TestFixture]
public class BudgetMapperTests
{
    private BudgetMapper _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new BudgetMapper(A.Fake<ILogger<BudgetMapper>>());
    }

    [Test]
    public void BuildBudgetPlan_WhenEntriesEmpty_ShouldReturnEmptyList()
    {
        var result = _sut.BuildBudgetPlan(new List<BudgetFlatEntryDto>());

        result.Should().BeEmpty();
    }

    [Test]
    public void BuildBudgetPlan_ShouldGroupByCostCenterAndCategoryAndSplitIncomeFromExpenses()
    {
        var entries = new List<BudgetFlatEntryDto>
        {
            Entry(costCenterId: 1, "Sports", categoryId: 10, "Equipment", amount: 100m),
            Entry(costCenterId: 1, "Sports", categoryId: 10, "Equipment", amount: -100m)
        };

        var result = _sut.BuildBudgetPlan(entries);

        result.Should().ContainSingle();
        var costCenter = result.Single();
        costCenter.CostCenterId.Should().Be(1);
        costCenter.CostCenterName.Should().Be("Sports");

        var category = costCenter.Categories.Should().ContainSingle().Subject;
        category.CategoryId.Should().Be(10);
        category.CategoryName.Should().Be("Equipment");
        category.Income.Should().Be(100m);
        category.Expenses.Should().Be(100m);
    }

    [Test]
    public void BuildBudgetPlan_ShouldRoundAmountsUpToNearestHundred()
    {
        var entries = new List<BudgetFlatEntryDto>
        {
            Entry(costCenterId: 1, "Sports", categoryId: 10, "Equipment", amount: 150m),
            Entry(costCenterId: 1, "Sports", categoryId: 10, "Equipment", amount: -250m)
        };

        var category = _sut.BuildBudgetPlan(entries).Single().Categories.Single();

        category.Income.Should().Be(200m);
        category.Expenses.Should().Be(300m);
    }

    [Test]
    public void BuildBudgetPlan_WhenNoIncomeOrExpenses_ShouldRoundToZero()
    {
        var entries = new List<BudgetFlatEntryDto>
        {
            Entry(costCenterId: 1, "Sports", categoryId: 10, "Equipment", amount: -50m)
        };

        var category = _sut.BuildBudgetPlan(entries).Single().Categories.Single();

        category.Income.Should().Be(0m);
        category.Expenses.Should().Be(100m);
    }

    [Test]
    public void BuildBudgetPlan_ShouldSetCostCenterTotalsToSumOfRoundedCategoryFigures()
    {
        var entries = new List<BudgetFlatEntryDto>
        {
            Entry(costCenterId: 1, "Sports", categoryId: 10, "Equipment", amount: 150m),
            Entry(costCenterId: 1, "Sports", categoryId: 11, "Travel", amount: 120m),
            Entry(costCenterId: 1, "Sports", categoryId: 11, "Travel", amount: -50m)
        };

        var costCenter = _sut.BuildBudgetPlan(entries).Single();

        // Equipment: income 200, expenses 0; Travel: income 200, expenses 100
        costCenter.Income.Should().Be(400m);
        costCenter.Expenses.Should().Be(100m);
    }

    [Test]
    public void BuildBudgetPlan_ShouldOrderCostCentersAndCategoriesByName()
    {
        var entries = new List<BudgetFlatEntryDto>
        {
            Entry(costCenterId: 1, "Sports", categoryId: 11, "Travel", amount: 100m),
            Entry(costCenterId: 1, "Sports", categoryId: 10, "Equipment", amount: 100m),
            Entry(costCenterId: 2, "Admin", categoryId: 20, "Office", amount: 100m)
        };

        var result = _sut.BuildBudgetPlan(entries);

        result.Select(cc => cc.CostCenterName).Should().ContainInOrder("Admin", "Sports");
        result.First(cc => cc.CostCenterName == "Sports").Categories
            .Select(cat => cat.CategoryName).Should().ContainInOrder("Equipment", "Travel");
    }

    private static BudgetFlatEntryDto Entry(
        int costCenterId, string costCenterName, int categoryId, string categoryName, decimal amount)
        => new()
        {
            CostCenterId = costCenterId,
            CostCenterName = costCenterName,
            CategoryId = categoryId,
            CategoryName = categoryName,
            Amount = amount
        };
}