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

namespace ClubTreasury.Tests.Components;

[TestFixture]
public class CashRegisterDialogTests
{
    private BunitContext _ctx = null!;
    private ICashRegisterService _cashRegisterService = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private INotificationService _notificationService = null!;
    private IOperationResultFactory _operationResultFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _ctx = new BunitContext();

        _cashRegisterService = A.Fake<ICashRegisterService>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();
        _notificationService = A.Fake<INotificationService>();
        _operationResultFactory = A.Fake<IOperationResultFactory>();

        A.CallTo(() => _localizer[A<string>._])
            .ReturnsLazily((string key) => new LocalizedString(key, key));

        _ctx.Services.AddSingleton(_cashRegisterService);
        _ctx.Services.AddSingleton(_localizer);
        _ctx.Services.AddSingleton(_notificationService);
        _ctx.Services.AddSingleton(_operationResultFactory);
        _ctx.Services.AddSingleton(new CashRegisterValidator(_localizer));
        _ctx.Services.AddMudServices();

        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [TearDown]
    public async Task TearDown()
    {
        await _ctx.DisposeAsync();
    }

    private IRenderedComponent<MudDialogProvider> RenderDialog(int? cashRegisterId = null)
    {
        var provider = _ctx.Render<MudDialogProvider>();
        var dialogService = _ctx.Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<CashRegisterDialog>();
        if (cashRegisterId.HasValue)
            parameters.Add(x => x.CashRegisterId, cashRegisterId);

        provider.InvokeAsync(() =>
            dialogService.ShowAsync<CashRegisterDialog>("Dialog", parameters));

        return provider;
    }

    [Test]
    public void AddMode_RendersWithAddEntryButton()
    {
        var provider = RenderDialog();

        provider.Markup.Should().Contain("AddEntry");
        provider.Markup.Should().Contain("Cancel");
    }

    [Test]
    public void EditMode_LoadsCashRegisterAndShowsSaveButton()
    {
        var cashRegister = new CashRegisterModel { Id = 1, Name = "Main", FiscalYearStartMonth = 7 };
        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1)).Returns(cashRegister);
        A.CallTo(() => _cashRegisterService.GetLogoAsync(1)).Returns(((byte[], string)?)null);

        var provider = RenderDialog(cashRegisterId: 1);

        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1)).MustHaveHappenedOnceExactly();
        provider.Markup.Should().Contain("Save");
    }

    [Test]
    public void EditMode_LoadsExistingLogo()
    {
        var cashRegister = new CashRegisterModel { Id = 1, Name = "Main", FiscalYearStartMonth = 7 };
        var logoData = new byte[] { 1, 2, 3 };
        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1)).Returns(cashRegister);
        A.CallTo(() => _cashRegisterService.GetLogoAsync(1)).Returns((logoData, "image/png"));

        var provider = RenderDialog(cashRegisterId: 1);

        // Logo should be rendered as a data URI image
        provider.Markup.Should().Contain("data:image/png;base64,");
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

        var cancelButton = provider.FindAll("button")
            .First(b => b.TextContent.Contains("Cancel"));
        cancelButton.Click();

        provider.Markup.Should().NotContain("Cancel");
    }

    [Test]
    public async Task SaveInEditMode_CallsUpdateOnSuccess()
    {
        var cashRegister = new CashRegisterModel { Id = 1, Name = "Main", FiscalYearStartMonth = 7 };
        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1)).Returns(cashRegister);
        A.CallTo(() => _cashRegisterService.GetLogoAsync(1)).Returns(((byte[], string)?)null);
        A.CallTo(() => _cashRegisterService.UpdateCashRegister(A<CashRegisterModel>._))
            .Returns(new OperationResult { Status = OperationResultStatus.Success });

        var provider = RenderDialog(cashRegisterId: 1);

        var saveButton = provider.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await provider.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _cashRegisterService.UpdateCashRegister(A<CashRegisterModel>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveInEditMode_ShowsNotificationOnFailure()
    {
        var cashRegister = new CashRegisterModel { Id = 1, Name = "Main", FiscalYearStartMonth = 7 };
        var failResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Update failed"
        };
        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1)).Returns(cashRegister);
        A.CallTo(() => _cashRegisterService.GetLogoAsync(1)).Returns(((byte[], string)?)null);
        A.CallTo(() => _cashRegisterService.UpdateCashRegister(A<CashRegisterModel>._))
            .Returns(failResult);

        var provider = RenderDialog(cashRegisterId: 1);

        var saveButton = provider.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await provider.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _notificationService.ShowOperationResultAsync(failResult)).MustHaveHappened();
    }

    [Test]
    public async Task SaveInEditMode_DeletesLogoWhenRemoved()
    {
        var cashRegister = new CashRegisterModel { Id = 1, Name = "Main", FiscalYearStartMonth = 7 };
        var logoData = new byte[] { 1, 2, 3 };
        A.CallTo(() => _cashRegisterService.GetCashRegisterById(1)).Returns(cashRegister);
        A.CallTo(() => _cashRegisterService.GetLogoAsync(1)).Returns((logoData, "image/png"));
        A.CallTo(() => _cashRegisterService.UpdateCashRegister(A<CashRegisterModel>._))
            .Returns(new OperationResult { Status = OperationResultStatus.Success });

        var provider = RenderDialog(cashRegisterId: 1);

        // Click the delete logo button (MudIconButton rendered with mud-icon-button class)
        var deleteButton = provider.Find("button.mud-icon-button");
        deleteButton.Click();

        // Logo image should be gone
        provider.Markup.Should().NotContain("data:image/png;base64,");

        // Now save
        var saveButton = provider.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await provider.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _cashRegisterService.DeleteLogoAsync(1)).MustHaveHappened();
    }
}
