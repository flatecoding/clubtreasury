using Bunit;
using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.Notification;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.ComponentTests.Components;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CostCenterDialogTests : BunitContext
{
    private ICostCenterService _costCenterService = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private INotificationService _notificationService = null!;
    private IResultFactory _resultFactory = null!;

    [SetUp]
    public void SetUp()
    {
        Services.AddSingleton(_costCenterService = A.Fake<ICostCenterService>());
        Services.AddSingleton(_localizer = A.Fake<IStringLocalizer<Translation>>());
        Services.AddSingleton(_notificationService = A.Fake<INotificationService>());
        Services.AddSingleton(_resultFactory = A.Fake<IResultFactory>());

        A.CallTo(() => _localizer[A<string>._])
            .ReturnsLazily((string key) => new LocalizedString(key, key));

        Services.AddSingleton(new CostCenterValidator(_localizer));
        Services.AddMudServices();

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<MudDialogProvider> RenderDialog(int? costCenterId = null)
    {
        var cut = Render<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<CostCenterDialog>();
        if (costCenterId.HasValue)
            parameters.Add(x => x.CostCenterId, costCenterId);

        cut.InvokeAsync(() =>
            dialogService.ShowAsync<CostCenterDialog>("Dialog", parameters));

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
    public void EditMode_LoadsCostCenterAndShowsSaveButton()
    {
        var costCenter = new CostCenterModel { Id = 1, CostUnitName = "Admin" };
        A.CallTo(() => _costCenterService.GetCostCenterByIdAsync(1)).Returns(costCenter);

        var cut = RenderDialog(costCenterId: 1);

        A.CallTo(() => _costCenterService.GetCostCenterByIdAsync(1)).MustHaveHappenedOnceExactly();
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
        var costCenter = new CostCenterModel { Id = 1, CostUnitName = "Admin" };
        A.CallTo(() => _costCenterService.GetCostCenterByIdAsync(1)).Returns(costCenter);
        A.CallTo(() => _costCenterService.UpdateCostCenterAsync(A<CostCenterModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog(costCenterId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _costCenterService.UpdateCostCenterAsync(A<CostCenterModel>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveInEditMode_ShowsNotificationOnFailure()
    {
        var costCenter = new CostCenterModel { Id = 1, CostUnitName = "Admin" };
        var failResult = Result.Failure(new Error("Test.Error", "Update failed"));
        A.CallTo(() => _costCenterService.GetCostCenterByIdAsync(1)).Returns(costCenter);
        A.CallTo(() => _costCenterService.UpdateCostCenterAsync(A<CostCenterModel>._))
            .Returns(failResult);

        var cut = RenderDialog(costCenterId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _notificationService.ShowResultAsync(failResult)).MustHaveHappened();
    }

    [Test]
    public void EditMode_InputContainsLoadedValue()
    {
        var costCenter = new CostCenterModel { Id = 1, CostUnitName = "Admin" };
        A.CallTo(() => _costCenterService.GetCostCenterByIdAsync(1)).Returns(costCenter);

        var cut = RenderDialog(costCenterId: 1);

        var input = cut.Find("input");
        input.GetAttribute("value").Should().Be("Admin");
    }

    [Test]
    public async Task AddMode_ValidationPreventsEmptySave()
    {
        var cut = RenderDialog();

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("AddEntry"));
        await cut.InvokeAsync(() => addButton.Click());

        A.CallTo(() => _costCenterService.AddCostCenterAsync(A<CostCenterModel>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task AddMode_CallsAddOnSuccess()
    {
        A.CallTo(() => _costCenterService.AddCostCenterAsync(A<CostCenterModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog();

        var input = cut.Find("input");
        input.Input("New CostCenter");

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("AddEntry"));
        await cut.InvokeAsync(() => addButton.Click());

        A.CallTo(() => _costCenterService.AddCostCenterAsync(
            A<CostCenterModel>.That.Matches(m => m.CostUnitName == "New CostCenter")))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveOnSuccess_ClosesDialog()
    {
        var costCenter = new CostCenterModel { Id = 1, CostUnitName = "Admin" };
        A.CallTo(() => _costCenterService.GetCostCenterByIdAsync(1)).Returns(costCenter);
        A.CallTo(() => _costCenterService.UpdateCostCenterAsync(A<CostCenterModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog(costCenterId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        cut.Markup.Should().NotContain("Save");
    }
}
