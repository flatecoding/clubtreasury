using Bunit;
using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using ClubTreasury.Data.ItemDetail;
using ClubTreasury.Data.Notification;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.ComponentTests.Components;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class ItemDetailDialogTests : BunitContext
{
    private IItemDetailService _itemDetailService = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private INotificationService _notificationService = null!;
    private IOperationResultFactory _operationResultFactory = null!;

    [SetUp]
    public void SetUp()
    {
        Services.AddSingleton(_itemDetailService = A.Fake<IItemDetailService>());
        Services.AddSingleton(_localizer = A.Fake<IStringLocalizer<Translation>>());
        Services.AddSingleton(_notificationService = A.Fake<INotificationService>());
        Services.AddSingleton(_operationResultFactory = A.Fake<IOperationResultFactory>());

        A.CallTo(() => _localizer[A<string>._])
            .ReturnsLazily((string key) => new LocalizedString(key, key));

        Services.AddSingleton(new ItemDetailValidator(_localizer));
        Services.AddMudServices();

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<MudDialogProvider> RenderDialog(int? itemDetailId = null)
    {
        var cut = Render<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<ItemDetailDialog>();
        if (itemDetailId.HasValue)
            parameters.Add(x => x.ItemDetailId, itemDetailId);

        cut.InvokeAsync(() =>
            dialogService.ShowAsync<ItemDetailDialog>("Dialog", parameters));

        return cut;
    }

    [Test]
    public void AddMode_RendersWithAddEntryButton()
    {
        var cut = RenderDialog();

        cut.Markup.Should().Contain("AddEntry");
        cut.Markup.Should().Contain("Cancel");
    }

    [Test]
    public void EditMode_LoadsItemDetailAndShowsSaveButton()
    {
        var itemDetail = new ItemDetailModel { Id = 1, CostDetails = "Office Supplies" };
        A.CallTo(() => _itemDetailService.GetItemDetailByIdAsync(1)).Returns(itemDetail);

        var cut = RenderDialog(itemDetailId: 1);

        A.CallTo(() => _itemDetailService.GetItemDetailByIdAsync(1)).MustHaveHappenedOnceExactly();
        cut.Markup.Should().Contain("Save");
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

        var cut = RenderDialog();

        var cancelButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Cancel"));
        cancelButton.Click();

        cut.Markup.Should().NotContain("Cancel");
    }

    [Test]
    public async Task SaveInEditMode_CallsUpdateOnSuccess()
    {
        var itemDetail = new ItemDetailModel { Id = 1, CostDetails = "Office Supplies" };
        A.CallTo(() => _itemDetailService.GetItemDetailByIdAsync(1)).Returns(itemDetail);
        A.CallTo(() => _itemDetailService.UpdateItemDetailAsync(A<ItemDetailModel>._))
            .Returns(new OperationResult { Status = OperationResultStatus.Success });

        var cut = RenderDialog(itemDetailId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _itemDetailService.UpdateItemDetailAsync(A<ItemDetailModel>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveInEditMode_ShowsNotificationOnFailure()
    {
        var itemDetail = new ItemDetailModel { Id = 1, CostDetails = "Office Supplies" };
        var failResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Update failed"
        };
        A.CallTo(() => _itemDetailService.GetItemDetailByIdAsync(1)).Returns(itemDetail);
        A.CallTo(() => _itemDetailService.UpdateItemDetailAsync(A<ItemDetailModel>._))
            .Returns(failResult);

        var cut = RenderDialog(itemDetailId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _notificationService.ShowOperationResultAsync(failResult)).MustHaveHappened();
    }

    [Test]
    public void EditMode_InputContainsLoadedValue()
    {
        var itemDetail = new ItemDetailModel { Id = 1, CostDetails = "Office Supplies" };
        A.CallTo(() => _itemDetailService.GetItemDetailByIdAsync(1)).Returns(itemDetail);

        var cut = RenderDialog(itemDetailId: 1);

        var input = cut.Find("input");
        input.GetAttribute("value").Should().Be("Office Supplies");
    }

    [Test]
    public async Task AddMode_ValidationPreventsEmptySave()
    {
        var cut = RenderDialog();

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("AddEntry"));
        await cut.InvokeAsync(() => addButton.Click());

        A.CallTo(() => _itemDetailService.AddItemDetailAsync(A<ItemDetailModel>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task AddMode_CallsAddOnSuccess()
    {
        A.CallTo(() => _itemDetailService.AddItemDetailAsync(A<ItemDetailModel>._))
            .Returns(new OperationResult { Status = OperationResultStatus.Success });

        var cut = RenderDialog();

        var input = cut.Find("input");
        input.Input("New Detail");

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("AddEntry"));
        await cut.InvokeAsync(() => addButton.Click());

        A.CallTo(() => _itemDetailService.AddItemDetailAsync(
            A<ItemDetailModel>.That.Matches(m => m.CostDetails == "New Detail")))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveOnSuccess_ClosesDialog()
    {
        var itemDetail = new ItemDetailModel { Id = 1, CostDetails = "Office Supplies" };
        A.CallTo(() => _itemDetailService.GetItemDetailByIdAsync(1)).Returns(itemDetail);
        A.CallTo(() => _itemDetailService.UpdateItemDetailAsync(A<ItemDetailModel>._))
            .Returns(new OperationResult { Status = OperationResultStatus.Success });

        var cut = RenderDialog(itemDetailId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        cut.Markup.Should().NotContain("Save");
    }
}