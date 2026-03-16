using Bunit;
using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.Notification;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.ComponentTests.Components;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CashRegisterDialogTests : BunitContext
{
    private ICashRegisterService _cashRegisterService = null!;
    private ICashRegisterLogoService _cashRegisterLogoService = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private INotificationService _notificationService = null!;
    private IResultFactory _resultFactory = null!;

    [SetUp]
    public void SetUp()
    {
        Services.AddSingleton(_cashRegisterService = A.Fake<ICashRegisterService>());
        Services.AddSingleton(_cashRegisterLogoService = A.Fake<ICashRegisterLogoService>());
        Services.AddSingleton(_localizer = A.Fake<IStringLocalizer<Translation>>());
        Services.AddSingleton(_notificationService = A.Fake<INotificationService>());
        Services.AddSingleton(_resultFactory = A.Fake<IResultFactory>());

        A.CallTo(() => _localizer[A<string>._])
            .ReturnsLazily((string key) => new LocalizedString(key, key));

        Services.AddSingleton(new CashRegisterValidator(_localizer));
        Services.AddMudServices();

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<MudDialogProvider> RenderDialog(int? cashRegisterId = null)
    {
        var cut = Render<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<CashRegisterDialog>();
        if (cashRegisterId.HasValue)
            parameters.Add(x => x.CashRegisterId, cashRegisterId);

        cut.InvokeAsync(() =>
            dialogService.ShowAsync<CashRegisterDialog>("Dialog", parameters));

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
    public void EditMode_LoadsCashRegisterAndShowsSaveButton()
    {
        var cashRegister = new CashRegisterModel { Id = 1, Name = "Main", FiscalYearStartMonth = 7 };
        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1)).Returns(cashRegister);
        A.CallTo(() => _cashRegisterLogoService.GetLogoAsync(1)).Returns(((byte[], string)?)null);

        var cut = RenderDialog(cashRegisterId: 1);

        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1)).MustHaveHappenedOnceExactly();
        cut.Markup.Should().Contain("Save");
    }

    [Test]
    public void EditMode_LoadsExistingLogo()
    {
        var cashRegister = new CashRegisterModel { Id = 1, Name = "Main", FiscalYearStartMonth = 7 };
        var logoData = new byte[] { 1, 2, 3 };
        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1)).Returns(cashRegister);
        A.CallTo(() => _cashRegisterLogoService.GetLogoAsync(1)).Returns((logoData, "image/png"));

        var cut = RenderDialog(cashRegisterId: 1);

        // Logo should be rendered as a data URI image
        cut.Markup.Should().Contain("data:image/png;base64,");
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
        var cashRegister = new CashRegisterModel { Id = 1, Name = "Main", FiscalYearStartMonth = 7 };
        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1)).Returns(cashRegister);
        A.CallTo(() => _cashRegisterLogoService.GetLogoAsync(1)).Returns(((byte[], string)?)null);
        A.CallTo(() => _cashRegisterService.UpdateCashRegister(A<CashRegisterModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog(cashRegisterId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _cashRegisterService.UpdateCashRegister(A<CashRegisterModel>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveInEditMode_ShowsNotificationOnFailure()
    {
        var cashRegister = new CashRegisterModel { Id = 1, Name = "Main", FiscalYearStartMonth = 7 };
        var failResult = Result.Failure(new Error("Test.Error", "Update failed"));
        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1)).Returns(cashRegister);
        A.CallTo(() => _cashRegisterLogoService.GetLogoAsync(1)).Returns(((byte[], string)?)null);
        A.CallTo(() => _cashRegisterService.UpdateCashRegister(A<CashRegisterModel>._))
            .Returns(failResult);

        var cut = RenderDialog(cashRegisterId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _notificationService.ShowResultAsync(failResult)).MustHaveHappened();
    }

    [Test]
    public async Task SaveInEditMode_DeletesLogoWhenRemoved()
    {
        var cashRegister = new CashRegisterModel { Id = 1, Name = "Main", FiscalYearStartMonth = 7 };
        var logoData = new byte[] { 1, 2, 3 };
        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1)).Returns(cashRegister);
        A.CallTo(() => _cashRegisterLogoService.GetLogoAsync(1)).Returns((logoData, "image/png"));
        A.CallTo(() => _cashRegisterService.UpdateCashRegister(A<CashRegisterModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog(cashRegisterId: 1);

        // Click the delete logo button (MudIconButton rendered with mud-icon-button class)
        var deleteButton = cut.Find("button.mud-icon-button");
        await deleteButton.ClickAsync();

        // Logo image should be gone
        cut.Markup.Should().NotContain("data:image/png;base64,");

        // Now save
        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _cashRegisterLogoService.DeleteLogoAsync(1)).MustHaveHappened();
    }
}