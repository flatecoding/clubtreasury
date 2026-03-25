using Bunit;
using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using ClubTreasury.Components.Dialog;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.ComponentTests.Components;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class ConfirmDeleteDialogTests : BunitContext
{
    private IStringLocalizer<Translation> _localizer = null!;
    private IResultFactory _resultFactory = null!;

    [SetUp]
    public void SetUp()
    {
        Services.AddSingleton(_localizer = A.Fake<IStringLocalizer<Translation>>());
        Services.AddSingleton(_resultFactory = A.Fake<IResultFactory>());
        Services.AddSingleton(A.Fake<ILogger<ConfirmDeleteDialog>>());

        A.CallTo(() => _localizer[A<string>._])
            .ReturnsLazily((string key) => new LocalizedString(key, key));

        Services.AddMudServices();

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<MudDialogProvider> RenderDialog(
        string entityName = "CostCenter",
        string itemName = "Admin",
        Func<Task<Result>>? onConfirm = null)
    {
        var cut = Render<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<ConfirmDeleteDialog>
        {
            { x => x.EntityName, entityName },
            { x => x.ItemName, itemName }
        };
        if (onConfirm != null)
            parameters.Add(x => x.OnConfirm, onConfirm);

        cut.InvokeAsync(() =>
            dialogService.ShowAsync<ConfirmDeleteDialog>("Dialog", parameters));

        return cut;
    }

    [Test]
    public void RendersDeleteQuestionAndItemName()
    {
        var cut = RenderDialog(entityName: "CostCenter", itemName: "Admin");

        cut.Markup.Should().Contain("DeleteQuestion");
        cut.Markup.Should().Contain("Admin");
    }

    [Test]
    public void Cancel_ClosesDialogWithCanceledResult()
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
    public async Task Confirm_InvokesOnConfirmCallbackAndClosesDialog()
    {
        var confirmCalled = false;
        var successResult = Result.Success("Deleted");
        Func<Task<Result>> onConfirm = () => { confirmCalled = true; return Task.FromResult(successResult); };

        var cut = RenderDialog(onConfirm: onConfirm);

        var deleteButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Delete"));
        await cut.InvokeAsync(() => deleteButton.Click());

        confirmCalled.Should().BeTrue();
    }

    [Test]
    public async Task Confirm_WhenCallbackThrows_ClosesWithFailedResult()
    {
        Func<Task<Result>> onConfirm = () => throw new InvalidOperationException("DB error");
        var failResult = Result.Failure(new Error("Test.Error", "Failed"));
        A.CallTo(() => _resultFactory.FailedToDelete(A<string>._)).Returns(failResult);

        var cut = RenderDialog(onConfirm: onConfirm);

        var deleteButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Delete"));
        await cut.InvokeAsync(() => deleteButton.Click());

        A.CallTo(() => _resultFactory.FailedToDelete(A<string>._)).MustHaveHappened();
    }

    [Test]
    public void RendersDeleteButtonAndCancelButton()
    {
        var cut = RenderDialog();

        var buttons = cut.FindAll("button");
        buttons.Should().Contain(b => b.TextContent.Contains("Delete"));
        buttons.Should().Contain(b => b.TextContent.Contains("Cancel"));
    }

    [Test]
    public void Cancel_DoesNotInvokeOnConfirmCallback()
    {
        var confirmCalled = false;
        Func<Task<Result>> onConfirm = () => { confirmCalled = true; return Task.FromResult(Result.Success()); };
        A.CallTo(() => _resultFactory.Canceled())
            .Returns(Result.Failure(Error.Canceled));

        var cut = RenderDialog(onConfirm: onConfirm);

        var cancelButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Cancel"));
        cancelButton.Click();

        confirmCalled.Should().BeFalse();
    }

    [Test]
    public async Task Confirm_ClosesDialog()
    {
        var successResult = Result.Success();
        Func<Task<Result>> onConfirm = () => Task.FromResult(successResult);

        var cut = RenderDialog(onConfirm: onConfirm);

        var deleteButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Delete"));
        await cut.InvokeAsync(() => deleteButton.Click());

        cut.Markup.Should().NotContain("Delete");
    }

    [Test]
    public void RendersEntityNameInDialog()
    {
        var cut = RenderDialog(entityName: "Person", itemName: "John Doe");

        cut.Markup.Should().Contain("John Doe");
    }
}