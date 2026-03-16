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

namespace ClubTreasury.ComponentTests.Components;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TransactionDialogTests : BunitContext
{
    private ITransactionFormService _formService = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private INotificationService _notificationService = null!;
    private IResultFactory _resultFactory = null!;

    private List<CashRegisterModel> _cashRegisters = null!;
    private List<CostCenterModel> _costCenters = null!;
    private List<SpecialItemModel> _specialItems = null!;

    [SetUp]
    public void SetUp()
    {
        Services.AddSingleton(_formService = A.Fake<ITransactionFormService>());
        Services.AddSingleton(_localizer = A.Fake<IStringLocalizer<Translation>>());
        Services.AddSingleton(_notificationService = A.Fake<INotificationService>());
        Services.AddSingleton(_resultFactory = A.Fake<IResultFactory>());

        A.CallTo(() => _localizer[A<string>._])
            .ReturnsLazily((string key) => new LocalizedString(key, key));

        _cashRegisters =
        [
            new CashRegisterModel { Id = 1, Name = "Main Register" },
            new CashRegisterModel { Id = 2, Name = "Secondary Register" }
        ];
        _costCenters = [new CostCenterModel { Id = 1, CostUnitName = "Admin" }];
        _specialItems = [new SpecialItemModel { Id = 1, Name = "Donation" }];

        A.CallTo(() => _formService.LoadReferenceDataAsync(A<CancellationToken>._))
            .Returns(new TransactionReferenceData(_cashRegisters, _costCenters, _specialItems));

        Services.AddSingleton(new TransactionValidator(_localizer));
        Services.AddMudServices();

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<MudDialogProvider> RenderDialog(int? transactionId = null)
    {
        var cut = Render<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TransactionDialog>();
        if (transactionId.HasValue)
            parameters.Add(x => x.TransactionId, transactionId);

        cut.InvokeAsync(() =>
            dialogService.ShowAsync<TransactionDialog>("Dialog", parameters));

        return cut;
    }

    [Test]
    public void AddMode_RendersAndLoadsReferenceData()
    {
        var cut = RenderDialog();

        A.CallTo(() => _formService.LoadReferenceDataAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        cut.Markup.Should().Contain("AddEntry");
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

        A.CallTo(() => _formService.LoadTransactionAsync(42, A<CancellationToken>._))
            .Returns(transaction);
        A.CallTo(() => _formService.GetCategoriesForCostCenterAsync(1, A<CancellationToken>._))
            .Returns(new List<CategoryModel> { category });
        A.CallTo(() => _formService.GetItemDetailsForCategoryAsync(10, A<CancellationToken>._))
            .Returns(new List<ItemDetailModel>());

        var cut = RenderDialog(transactionId: 42);

        A.CallTo(() => _formService.LoadTransactionAsync(42, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        cut.Markup.Should().Contain("Save");
    }

    [Test]
    public void Cancel_ClosesDialog()
    {
        var canceledResult = Result.Failure(Error.Canceled with { Message = "Canceled" });
        A.CallTo(() => _resultFactory.Canceled()).Returns(canceledResult);

        var cut = RenderDialog();

        var cancelButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Cancel"));
        cancelButton.Click();

        cut.Markup.Should().NotContain("Cancel");
    }

    [Test]
    public async Task SaveInEditMode_CallsFormServiceSave()
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

        A.CallTo(() => _formService.LoadTransactionAsync(42, A<CancellationToken>._))
            .Returns(transaction);
        A.CallTo(() => _formService.GetCategoriesForCostCenterAsync(1, A<CancellationToken>._))
            .Returns(new List<CategoryModel> { category });
        A.CallTo(() => _formService.GetItemDetailsForCategoryAsync(10, A<CancellationToken>._))
            .Returns(new List<ItemDetailModel>());
        A.CallTo(() => _formService.SaveTransactionAsync(
                A<TransactionModel>._, A<TransactionFormSelections>._, true, A<CancellationToken>._))
            .Returns(Result.Success());

        var cut = RenderDialog(transactionId: 42);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _formService.SaveTransactionAsync(
                A<TransactionModel>._, A<TransactionFormSelections>._, true, A<CancellationToken>._))
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

        var failResult = Result.Failure(new Error("Test.Error", "Update failed"));

        A.CallTo(() => _formService.LoadTransactionAsync(42, A<CancellationToken>._))
            .Returns(transaction);
        A.CallTo(() => _formService.GetCategoriesForCostCenterAsync(1, A<CancellationToken>._))
            .Returns(new List<CategoryModel> { category });
        A.CallTo(() => _formService.GetItemDetailsForCategoryAsync(10, A<CancellationToken>._))
            .Returns([]);
        A.CallTo(() => _formService.SaveTransactionAsync(
                A<TransactionModel>._, A<TransactionFormSelections>._, true, A<CancellationToken>._))
            .Returns(failResult);

        var cut = RenderDialog(transactionId: 42);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _notificationService.ShowResultAsync(failResult)).MustHaveHappened();
    }

    [Test]
    public void CashRegisterChange_CallsGetNextDocumentNumber()
    {
        RenderDialog();

        A.CallTo(() => _formService.GetNextDocumentNumberAsync(A<int>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
