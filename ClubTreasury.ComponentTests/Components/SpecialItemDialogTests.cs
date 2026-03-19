using Bunit;
using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using ClubTreasury.Data.Notification;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.SpecialItem;

namespace ClubTreasury.ComponentTests.Components;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class SpecialItemDialogTests : BunitContext
{
    private ISpecialItemService _specialItemService = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private INotificationService _notificationService = null!;
    private IResultFactory _resultFactory = null!;

    [SetUp]
    public void SetUp()
    {
        Services.AddSingleton(_specialItemService = A.Fake<ISpecialItemService>());
        Services.AddSingleton(_localizer = A.Fake<IStringLocalizer<Translation>>());
        Services.AddSingleton(_notificationService = A.Fake<INotificationService>());
        Services.AddSingleton(_resultFactory = A.Fake<IResultFactory>());

        A.CallTo(() => _localizer[A<string>._])
            .ReturnsLazily((string key) => new LocalizedString(key, key));

        Services.AddSingleton(new SpecialItemValidator(_localizer));
        Services.AddMudServices();

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<MudDialogProvider> RenderDialog(int? specialItemId = null)
    {
        var cut = Render<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<SpecialItemDialog>();
        if (specialItemId.HasValue)
            parameters.Add(x => x.SpecialItemId, specialItemId);

        cut.InvokeAsync(() =>
            dialogService.ShowAsync<SpecialItemDialog>("Dialog", parameters));

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
    public void EditMode_LoadsSpecialItemAndShowsSaveButton()
    {
        var specialItem = new SpecialItemModel { Id = 1, Name = "Donation" };
        A.CallTo(() => _specialItemService.GetSpecialPositionByIdAsync(1)).Returns(specialItem);

        var cut = RenderDialog(specialItemId: 1);

        A.CallTo(() => _specialItemService.GetSpecialPositionByIdAsync(1)).MustHaveHappenedOnceExactly();
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
        var specialItem = new SpecialItemModel { Id = 1, Name = "Donation" };
        A.CallTo(() => _specialItemService.GetSpecialPositionByIdAsync(1)).Returns(specialItem);
        A.CallTo(() => _specialItemService.UpdateSpecialPositionAsync(A<SpecialItemModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog(specialItemId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _specialItemService.UpdateSpecialPositionAsync(A<SpecialItemModel>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveInEditMode_ShowsNotificationOnFailure()
    {
        var specialItem = new SpecialItemModel { Id = 1, Name = "Donation" };
        var failResult = Result.Failure(new Error("Test.Error", "Update failed"));
        A.CallTo(() => _specialItemService.GetSpecialPositionByIdAsync(1)).Returns(specialItem);
        A.CallTo(() => _specialItemService.UpdateSpecialPositionAsync(A<SpecialItemModel>._))
            .Returns(failResult);

        var cut = RenderDialog(specialItemId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _notificationService.ShowResultAsync(failResult)).MustHaveHappened();
    }

    [Test]
    public void EditMode_InputContainsLoadedValue()
    {
        var specialItem = new SpecialItemModel { Id = 1, Name = "Donation" };
        A.CallTo(() => _specialItemService.GetSpecialPositionByIdAsync(1)).Returns(specialItem);

        var cut = RenderDialog(specialItemId: 1);

        var input = cut.Find("input");
        input.GetAttribute("value").Should().Be("Donation");
    }

    [Test]
    public async Task AddMode_ValidationPreventsEmptySave()
    {
        var cut = RenderDialog();

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("AddEntry"));
        await cut.InvokeAsync(() => addButton.Click());

        A.CallTo(() => _specialItemService.AddSpecialPositionAsync(A<SpecialItemModel>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task AddMode_CallsAddOnSuccess()
    {
        A.CallTo(() => _specialItemService.AddSpecialPositionAsync(A<SpecialItemModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog();

        var input = cut.Find("input");
        input.Input("Grant");

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("AddEntry"));
        await cut.InvokeAsync(() => addButton.Click());

        A.CallTo(() => _specialItemService.AddSpecialPositionAsync(
            A<SpecialItemModel>.That.Matches(m => m.Name == "Grant")))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveOnSuccess_ClosesDialog()
    {
        var specialItem = new SpecialItemModel { Id = 1, Name = "Donation" };
        A.CallTo(() => _specialItemService.GetSpecialPositionByIdAsync(1)).Returns(specialItem);
        A.CallTo(() => _specialItemService.UpdateSpecialPositionAsync(A<SpecialItemModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog(specialItemId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        cut.Markup.Should().NotContain("Save");
    }
}
