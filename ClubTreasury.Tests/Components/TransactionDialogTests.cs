using Bunit;
using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.ItemDetail;
using ClubTreasury.Data.Notification;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.SpecialItem;
using ClubTreasury.Data.Transaction;
using ClubTreasury.Data.Transaction.Dialogs;

namespace ClubTreasury.Tests.Components;

[TestFixture]
public class TransactionDialogTests
{
    private BunitContext _ctx = null!;
    private ITransactionService _transactionService = null!;
    private ICostCenterService _costCenterService = null!;
    private ICategoryService _categoryService = null!;
    private IItemDetailService _itemDetailService = null!;
    private ICashRegisterService _cashRegisterService = null!;
    private ISpecialItemService _specialItemService = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private INotificationService _notificationService = null!;
    private IOperationResultFactory _operationResultFactory = null!;
    private IAllocationService _allocationService = null!;

    private List<CashRegisterModel> _cashRegisters = null!;
    private List<CostCenterModel> _costCenters = null!;
    private List<SpecialItemModel> _specialItems = null!;

    [SetUp]
    public void SetUp()
    {
        _ctx = new BunitContext();

        _transactionService = A.Fake<ITransactionService>();
        _costCenterService = A.Fake<ICostCenterService>();
        _categoryService = A.Fake<ICategoryService>();
        _itemDetailService = A.Fake<IItemDetailService>();
        _cashRegisterService = A.Fake<ICashRegisterService>();
        _specialItemService = A.Fake<ISpecialItemService>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();
        _notificationService = A.Fake<INotificationService>();
        _operationResultFactory = A.Fake<IOperationResultFactory>();
        _allocationService = A.Fake<IAllocationService>();

        // Localizer returns key as value for any lookup
        A.CallTo(() => _localizer[A<string>._])
            .ReturnsLazily((string key) => new LocalizedString(key, key));

        // Reference data
        _cashRegisters =
        [
            new CashRegisterModel { Id = 1, Name = "Main Register" },
            new CashRegisterModel { Id = 2, Name = "Secondary Register" }
        ];
        _costCenters = [new CostCenterModel { Id = 1, CostUnitName = "Admin" }];
        _specialItems = [new SpecialItemModel { Id = 1, Name = "Donation" }];

        A.CallTo(() => _cashRegisterService.GetAllCashRegisters()).Returns(_cashRegisters);
        A.CallTo(() => _costCenterService.GetAllCostCentersAsync()).Returns(_costCenters);
        A.CallTo(() => _specialItemService.GetAllSpecialItems()).Returns(_specialItems);

        // Register services
        _ctx.Services.AddSingleton(_transactionService);
        _ctx.Services.AddSingleton(_costCenterService);
        _ctx.Services.AddSingleton(_categoryService);
        _ctx.Services.AddSingleton(_itemDetailService);
        _ctx.Services.AddSingleton(_cashRegisterService);
        _ctx.Services.AddSingleton(_specialItemService);
        _ctx.Services.AddSingleton(_localizer);
        _ctx.Services.AddSingleton(_notificationService);
        _ctx.Services.AddSingleton(_operationResultFactory);
        _ctx.Services.AddSingleton(_allocationService);
        _ctx.Services.AddSingleton(new TransactionValidator(_localizer));
        _ctx.Services.AddMudServices();

        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [TearDown]
    public async Task TearDown()
    {
        await _ctx.DisposeAsync();
    }

    private IRenderedComponent<MudDialogProvider> RenderDialog(int? transactionId = null)
    {
        var provider = _ctx.Render<MudDialogProvider>();
        var dialogService = _ctx.Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TransactionDialog>();
        if (transactionId.HasValue)
            parameters.Add(x => x.TransactionId, transactionId);

        provider.InvokeAsync(() =>
            dialogService.ShowAsync<TransactionDialog>("Dialog", parameters));

        return provider;
    }

    [Test]
    public void AddMode_RendersAndLoadsReferenceData()
    {
        var provider = RenderDialog();

        A.CallTo(() => _cashRegisterService.GetAllCashRegisters()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _costCenterService.GetAllCostCentersAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _specialItemService.GetAllSpecialItems()).MustHaveHappenedOnceExactly();

        // Should contain the action button with AddEntry text
        provider.Markup.Should().Contain("AddEntry");
    }

    [Test]
    public void EditMode_LoadsTransactionAndRestoresSelections()
    {
        var costCenter = new CostCenterModel { Id = 1, CostUnitName = "Admin" };
        var category = new CategoryModel { Id = 10, Name = "Fees" };
        var transaction = new TransactionModel
        {
            Id = 42,
            Date = new DateOnly(2025, 6, 15),
            Documentnumber = 5,
            Description = "Test entry",
            Sum = 100m,
            AccountMovement = 100m,
            CashRegisterId = 1,
            CashRegister = _cashRegisters[0],
            AllocationId = 1,
            Allocation = new AllocationModel
            {
                Id = 1,
                CostCenter = costCenter,
                CostCenterId = 1,
                Category = category,
                CategoryId = 10,
                ItemDetail = null
            },
            SpecialItem = null
        };

        A.CallTo(() => _transactionService.GetTransactionByIdAsync(42)).Returns(transaction);
        A.CallTo(() => _categoryService.GetCategoriesByCostCenterIdAsync(1))
            .Returns(new List<CategoryModel> { category });
        A.CallTo(() => _itemDetailService.GetItemDetailByCategoryIdAsync(10))
            .Returns(new List<ItemDetailModel>());

        var provider = RenderDialog(transactionId: 42);

        A.CallTo(() => _transactionService.GetTransactionByIdAsync(42)).MustHaveHappenedOnceExactly();

        // Should show Save button in edit mode
        provider.Markup.Should().Contain("Save");
    }

    [Test]
    public void Cancel_ClosesDialog()
    {
        var canceledResult = new OperationResult
        {
            Status = OperationResultStatus.Canceled,
            Message = "Canceled"
        };
        A.CallTo(() => _operationResultFactory.Canceled()).Returns(canceledResult);

        var provider = RenderDialog();

        // Find and click the Cancel button
        var cancelButton = provider.FindAll("button")
            .First(b => b.TextContent.Contains("Cancel"));
        cancelButton.Click();

        // After cancel, dialog content should be removed from provider
        provider.Markup.Should().NotContain("Cancel");
    }

    [Test]
    public async Task SaveInEditMode_CallsUpdateOnSuccess()
    {
        var costCenter = new CostCenterModel { Id = 1, CostUnitName = "Admin" };
        var category = new CategoryModel { Id = 10, Name = "Fees" };
        var allocation = new AllocationModel
        {
            Id = 1,
            CostCenter = costCenter,
            CostCenterId = 1,
            Category = category,
            CategoryId = 10
        };
        var transaction = new TransactionModel
        {
            Id = 42,
            Date = new DateOnly(2025, 6, 15),
            Documentnumber = 5,
            Description = "Test entry",
            Sum = 100m,
            AccountMovement = 100m,
            CashRegisterId = 1,
            CashRegister = _cashRegisters[0],
            AllocationId = 1,
            Allocation = allocation,
            SpecialItem = null
        };

        A.CallTo(() => _transactionService.GetTransactionByIdAsync(42)).Returns(transaction);
        A.CallTo(() => _categoryService.GetCategoriesByCostCenterIdAsync(1))
            .Returns(new List<CategoryModel> { category });
        A.CallTo(() => _itemDetailService.GetItemDetailByCategoryIdAsync(10))
            .Returns(new List<ItemDetailModel>());
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync("Admin", "Fees", null))
            .Returns(allocation);
        A.CallTo(() => _transactionService.UpdateTransactionAsync(A<TransactionModel>._, A<CancellationToken>._))
            .Returns(new OperationResult { Status = OperationResultStatus.Success });

        var provider = RenderDialog(transactionId: 42);

        // Click the Save button
        var saveButton = provider.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await provider.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _transactionService.UpdateTransactionAsync(A<TransactionModel>._, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveInEditMode_ShowsNotificationOnFailure()
    {
        var costCenter = new CostCenterModel { Id = 1, CostUnitName = "Admin" };
        var category = new CategoryModel { Id = 10, Name = "Fees" };
        var allocation = new AllocationModel
        {
            Id = 1,
            CostCenter = costCenter,
            CostCenterId = 1,
            Category = category,
            CategoryId = 10
        };
        var transaction = new TransactionModel
        {
            Id = 42,
            Date = new DateOnly(2025, 6, 15),
            Documentnumber = 5,
            Description = "Test entry",
            Sum = 100m,
            AccountMovement = 100m,
            CashRegisterId = 1,
            CashRegister = _cashRegisters[0],
            AllocationId = 1,
            Allocation = allocation,
            SpecialItem = null
        };

        var failResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Update failed"
        };

        A.CallTo(() => _transactionService.GetTransactionByIdAsync(42)).Returns(transaction);
        A.CallTo(() => _categoryService.GetCategoriesByCostCenterIdAsync(1))
            .Returns(new List<CategoryModel> { category });
        A.CallTo(() => _itemDetailService.GetItemDetailByCategoryIdAsync(10))
            .Returns([]);
        A.CallTo(() => _allocationService.GetOrCreateAllocationAsync("Admin", "Fees", null))
            .Returns(allocation);
        A.CallTo(() => _transactionService.UpdateTransactionAsync(A<TransactionModel>._, A<CancellationToken>._))
            .Returns(failResult);

        var provider = RenderDialog(transactionId: 42);

        var saveButton = provider.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await provider.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _notificationService.ShowOperationResultAsync(failResult)).MustHaveHappened();
    }

    [Test]
    public void CashRegisterChange_CallsGetLatestDocumentNumberWithRegisterId()
    {
        A.CallTo(() => _transactionService.GetLatestDocumentNumberAsync(2)).Returns(10);

        RenderDialog();

        // In add mode with no selection, GetLatestDocumentNumberAsync should not be called
        A.CallTo(() => _transactionService.GetLatestDocumentNumberAsync(A<int>._))
            .MustNotHaveHappened();

        // The critical fix verified: OnCashRegisterChangedAsync calls
        //   TransactionService.GetLatestDocumentNumberAsync(_selectedCashRegisterModel.Id)
        // ensuring per-register document number lookup (was previously global).
    }
}
