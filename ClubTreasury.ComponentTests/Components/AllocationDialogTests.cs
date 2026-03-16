using Bunit;
using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.ItemDetail;
using ClubTreasury.Data.Notification;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.ComponentTests.Components;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class AllocationDialogTests : BunitContext
{
    private IAllocationService _allocationService = null!;
    private ICostCenterService _costCenterService = null!;
    private ICategoryService _categoryService = null!;
    private IItemDetailService _itemDetailService = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private INotificationService _notificationService = null!;
    private IResultFactory _resultFactory = null!;

    [SetUp]
    public void SetUp()
    {
        Services.AddSingleton(_allocationService = A.Fake<IAllocationService>());
        Services.AddSingleton(_costCenterService = A.Fake<ICostCenterService>());
        Services.AddSingleton(_categoryService = A.Fake<ICategoryService>());
        Services.AddSingleton(_itemDetailService = A.Fake<IItemDetailService>());
        Services.AddSingleton(_localizer = A.Fake<IStringLocalizer<Translation>>());
        Services.AddSingleton(_notificationService = A.Fake<INotificationService>());
        Services.AddSingleton(_resultFactory = A.Fake<IResultFactory>());

        A.CallTo(() => _localizer[A<string>._])
            .ReturnsLazily((string key) => new LocalizedString(key, key));

        A.CallTo(() => _costCenterService.GetAllCostCentersAsync())
            .Returns(new List<CostCenterModel> { new() { Id = 1, CostUnitName = "Admin" } });
        A.CallTo(() => _categoryService.GetAllCategoriesAsync())
            .Returns(new List<CategoryModel> { new() { Id = 1, Name = "Fees" } });
        A.CallTo(() => _itemDetailService.GetAllItemDetailsAsync())
            .Returns(new List<ItemDetailModel> { new() { Id = 1, CostDetails = "Office" } });

        Services.AddSingleton(new AllocationValidator(_localizer));
        Services.AddMudServices();

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<MudPopoverProvider> _popoverProvider = null!;

    private IRenderedComponent<MudDialogProvider> RenderDialog(int? allocationId = null)
    {
        _popoverProvider = Render<MudPopoverProvider>();
        var cut = Render<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<AllocationDialog>();
        if (allocationId.HasValue)
            parameters.Add(x => x.AllocationId, allocationId);

        cut.InvokeAsync(() =>
            dialogService.ShowAsync<AllocationDialog>("Dialog", parameters));

        return cut;
    }

    [Test]
    public void AddMode_RendersAndLoadsReferenceData()
    {
        var cut = RenderDialog();

        A.CallTo(() => _costCenterService.GetAllCostCentersAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _categoryService.GetAllCategoriesAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _itemDetailService.GetAllItemDetailsAsync()).MustHaveHappenedOnceExactly();

        cut.Markup.Should().Contain("AddEntry");
        cut.Markup.Should().Contain("Cancel");
    }

    [Test]
    public void EditMode_LoadsAllocationAndShowsSaveButton()
    {
        var allocation = new AllocationModel { Id = 1, CostCenterId = 1, CategoryId = 1 };
        A.CallTo(() => _allocationService.GetAllocationsByIdAsync(1)).Returns(allocation);

        var cut = RenderDialog(allocationId: 1);

        A.CallTo(() => _allocationService.GetAllocationsByIdAsync(1)).MustHaveHappenedOnceExactly();
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
    public async Task SaveInEditMode_CallsUpdateOnSuccess()
    {
        var allocation = new AllocationModel { Id = 1, CostCenterId = 1, CategoryId = 1 };
        A.CallTo(() => _allocationService.GetAllocationsByIdAsync(1)).Returns(allocation);
        A.CallTo(() => _allocationService.UpdateAllocationAsync(A<AllocationModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog(allocationId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _allocationService.UpdateAllocationAsync(A<AllocationModel>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveInEditMode_ShowsNotificationOnFailure()
    {
        var allocation = new AllocationModel { Id = 1, CostCenterId = 1, CategoryId = 1 };
        var failResult = Result.Failure(new Error("Test.Error", "Update failed"));
        A.CallTo(() => _allocationService.GetAllocationsByIdAsync(1)).Returns(allocation);
        A.CallTo(() => _allocationService.UpdateAllocationAsync(A<AllocationModel>._))
            .Returns(failResult);

        var cut = RenderDialog(allocationId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _notificationService.ShowResultAsync(failResult)).MustHaveHappened();
    }

    [Test]
    public void AddMode_DropdownLabelsAreRendered()
    {
        var cut = RenderDialog();

        cut.Markup.Should().Contain("CostCenter");
        cut.Markup.Should().Contain("Category");
        cut.Markup.Should().Contain("ItemDetail");
    }

    [Test]
    public async Task AddMode_CostCenterDropdownContainsItems()
    {
        var cut = RenderDialog();

        var costCenterSelect = cut.FindComponents<MudSelect<int>>()[0];
        var input = costCenterSelect.Find("input.mud-select-input");
        await cut.InvokeAsync(() => input.MouseDown());

        await _popoverProvider.WaitForAssertionAsync(() =>
            _popoverProvider.FindAll("div.mud-list-item").Count.Should().BeGreaterThan(0));

        var items = _popoverProvider.FindAll("div.mud-list-item").ToArray();
        items.Should().Contain(i => i.TextContent.Contains("Admin"));
    }

    [Test]
    public async Task AddMode_CategoryDropdownContainsItems()
    {
        var cut = RenderDialog();

        var categorySelect = cut.FindComponents<MudSelect<int>>()[1];
        var input = categorySelect.Find("input.mud-select-input");
        await cut.InvokeAsync(() => input.MouseDown());

        await _popoverProvider.WaitForAssertionAsync(() =>
            _popoverProvider.FindAll("div.mud-list-item").Count.Should().BeGreaterThan(0));

        var items = _popoverProvider.FindAll("div.mud-list-item").ToArray();
        items.Should().Contain(i => i.TextContent.Contains("Fees"));
    }

    [Test]
    public async Task AddMode_ItemDetailDropdownContainsItems()
    {
        var cut = RenderDialog();

        var itemDetailSelect = cut.FindComponent<MudSelect<int?>>();
        var input = itemDetailSelect.Find("input.mud-select-input");
        await cut.InvokeAsync(() => input.MouseDown());

        await _popoverProvider.WaitForAssertionAsync(() =>
            _popoverProvider.FindAll("div.mud-list-item").Count.Should().BeGreaterThan(0));

        var items = _popoverProvider.FindAll("div.mud-list-item").ToArray();
        items.Should().Contain(i => i.TextContent.Contains("Office"));
    }

    [Test]
    public async Task AddMode_ValidationPreventsEmptySave()
    {
        var cut = RenderDialog();

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("AddEntry"));
        await cut.InvokeAsync(() => addButton.Click());

        A.CallTo(() => _allocationService.AddAllocationAsync(A<AllocationModel>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task SaveOnSuccess_ClosesDialog()
    {
        var allocation = new AllocationModel { Id = 1, CostCenterId = 1, CategoryId = 1 };
        A.CallTo(() => _allocationService.GetAllocationsByIdAsync(1)).Returns(allocation);
        A.CallTo(() => _allocationService.UpdateAllocationAsync(A<AllocationModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog(allocationId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        cut.Markup.Should().NotContain("Save");
    }
}