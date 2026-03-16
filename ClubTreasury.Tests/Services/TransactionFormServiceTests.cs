using FakeItEasy;
using AwesomeAssertions;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.ItemDetail;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.SpecialItem;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Tests.Services;

[TestFixture]
public class TransactionFormServiceTests
{
    private ITransactionService _transactionService = null!;
    private ICashRegisterService _cashRegisterService = null!;
    private ICostCenterService _costCenterService = null!;
    private ICategoryService _categoryService = null!;
    private IItemDetailService _itemDetailService = null!;
    private ISpecialItemService _specialItemService = null!;
    private IAllocationService _allocationService = null!;
    private TransactionFormService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _transactionService = A.Fake<ITransactionService>();
        _cashRegisterService = A.Fake<ICashRegisterService>();
        _costCenterService = A.Fake<ICostCenterService>();
        _categoryService = A.Fake<ICategoryService>();
        _itemDetailService = A.Fake<IItemDetailService>();
        _specialItemService = A.Fake<ISpecialItemService>();
        _allocationService = A.Fake<IAllocationService>();

        _sut = new TransactionFormService(
            _transactionService,
            _cashRegisterService,
            _costCenterService,
            _categoryService,
            _itemDetailService,
            _specialItemService,
            _allocationService);
    }

    #region LoadReferenceDataAsync Tests

    [Test]
    public async Task LoadReferenceDataAsync_ShouldReturnAllReferenceData()
    {
        var cashRegisters = new List<CashRegisterModel> { new() { Id = 1, Name = "Main" } };
        var costCenters = new List<CostCenterModel> { new() { Id = 1, CostUnitName = "Admin" } };
        var specialItems = new List<SpecialItemModel> { new() { Id = 1, Name = "Donation" } };

        A.CallTo(() => _cashRegisterService.GetAllCashRegisters(A<CancellationToken>._)).Returns(cashRegisters);
        A.CallTo(() => _costCenterService.GetAllCostCentersAsync(A<CancellationToken>._)).Returns(costCenters);
        A.CallTo(() => _specialItemService.GetAllSpecialItems(A<CancellationToken>._)).Returns(specialItems);

        var result = await _sut.LoadReferenceDataAsync();

        result.CashRegisters.Should().BeSameAs(cashRegisters);
        result.CostCenters.Should().BeSameAs(costCenters);
        result.SpecialItems.Should().BeSameAs(specialItems);
    }

    #endregion

    #region LoadTransactionAsync Tests

    [Test]
    public async Task LoadTransactionAsync_ShouldDelegateToTransactionService()
    {
        var transaction = new TransactionModel { Id = 1 };
        A.CallTo(() => _transactionService.GetTransactionByIdAsync(1, A<CancellationToken>._))
            .Returns(transaction);

        var result = await _sut.LoadTransactionAsync(1);

        result.Should().BeSameAs(transaction);
    }

    #endregion

    #region GetCategoriesForCostCenterAsync Tests

    [Test]
    public async Task GetCategoriesForCostCenterAsync_ShouldReturnCategoriesAsList()
    {
        var categories = new List<CategoryModel> { new() { Id = 1, Name = "Fees" } };
        A.CallTo(() => _categoryService.GetCategoriesByCostCenterIdAsync(5, A<CancellationToken>._))
            .Returns(categories);

        var result = await _sut.GetCategoriesForCostCenterAsync(5);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Fees");
    }

    #endregion

    #region GetNextDocumentNumberAsync Tests

    [Test]
    public async Task GetNextDocumentNumberAsync_ShouldReturnLatestPlusOne()
    {
        A.CallTo(() => _transactionService.GetLatestDocumentNumberAsync(1, A<CancellationToken>._))
            .Returns(10);

        var result = await _sut.GetNextDocumentNumberAsync(1);

        result.Should().Be(11);
    }

    [Test]
    public async Task GetNextDocumentNumberAsync_WhenNoTransactions_ShouldReturnOne()
    {
        A.CallTo(() => _transactionService.GetLatestDocumentNumberAsync(1, A<CancellationToken>._))
            .Returns(0);

        var result = await _sut.GetNextDocumentNumberAsync(1);

        result.Should().Be(1);
    }

    #endregion

    #region SaveTransactionAsync Tests

    [Test]
    public async Task SaveTransactionAsync_InAddMode_ShouldMapAndCallAdd()
    {
        var cashRegister = new CashRegisterModel { Id = 1, Name = "Main" };
        var costCenter = new CostCenterModel { Id = 2, CostUnitName = "Admin" };
        var category = new CategoryModel { Id = 3, Name = "Fees" };
        var allocation = new AllocationModel { Id = 10 };
        var model = new TransactionModel { Description = "Test" };

        var selections = new TransactionFormSelections(
            cashRegister, costCenter, category, null, null, new DateTime(2025, 6, 15));

        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync("Admin", "Fees", null, A<CancellationToken>._))
            .Returns(allocation);
        A.CallTo(() => _transactionService.AddTransactionAsync(A<TransactionModel>._, A<CancellationToken>._))
            .Returns(Result.Success());

        var result = await _sut.SaveTransactionAsync(model, selections, isEditMode: false);

        result.IsSuccess.Should().BeTrue();
        model.CashRegisterId.Should().Be(1);
        model.AllocationId.Should().Be(10);
        model.Date.Should().Be(new DateOnly(2025, 6, 15));
        A.CallTo(() => _transactionService.AddTransactionAsync(model, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task SaveTransactionAsync_InEditMode_ShouldMapAndCallUpdate()
    {
        var cashRegister = new CashRegisterModel { Id = 1, Name = "Main" };
        var costCenter = new CostCenterModel { Id = 2, CostUnitName = "Admin" };
        var category = new CategoryModel { Id = 3, Name = "Fees" };
        var itemDetail = new ItemDetailModel { Id = 4, CostDetails = "License" };
        var specialItem = new SpecialItemModel { Id = 5, Name = "Donation" };
        var allocation = new AllocationModel { Id = 10 };
        var model = new TransactionModel { Id = 42, Description = "Test" };

        var selections = new TransactionFormSelections(
            cashRegister, costCenter, category, itemDetail, specialItem, new DateTime(2025, 6, 15));

        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync("Admin", "Fees", "License", A<CancellationToken>._))
            .Returns(allocation);
        A.CallTo(() => _transactionService.UpdateTransactionAsync(A<TransactionModel>._, A<CancellationToken>._))
            .Returns(Result.Success());

        var result = await _sut.SaveTransactionAsync(model, selections, isEditMode: true);

        result.IsSuccess.Should().BeTrue();
        model.SpecialItemId.Should().Be(5);
        A.CallTo(() => _transactionService.UpdateTransactionAsync(model, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task SaveTransactionAsync_WhenServiceFails_ShouldReturnFailure()
    {
        var selections = new TransactionFormSelections(
            new CashRegisterModel { Id = 1 },
            new CostCenterModel { Id = 2, CostUnitName = "Admin" },
            new CategoryModel { Id = 3, Name = "Fees" },
            null, null, new DateTime(2025, 6, 15));

        var allocation = new AllocationModel { Id = 10 };
        var failure = Result.Failure(new Error("Test.Error", "Failed"));

        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync(A<string>._, A<string>._, A<string?>._, A<CancellationToken>._))
            .Returns(allocation);
        A.CallTo(() => _transactionService.AddTransactionAsync(A<TransactionModel>._, A<CancellationToken>._))
            .Returns(failure);

        var result = await _sut.SaveTransactionAsync(new TransactionModel(), selections, isEditMode: false);

        result.IsFailure.Should().BeTrue();
    }

    #endregion
}
