using Bunit;
using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using ClubTreasury.Data.Notification;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.Person;
using ClubTreasury.Data.TransactionDetails;

namespace ClubTreasury.ComponentTests.Components;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TransactionDetailsDialogTests : BunitContext
{
    private ITransactionDetailsService _transactionDetailsService = null!;
    private IPersonService _personService = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private INotificationService _notificationService = null!;
    private IOperationResultFactory _operationResultFactory = null!;

    private List<PersonModel> _persons = null!;

    [SetUp]
    public void SetUp()
    {
        Services.AddSingleton(_transactionDetailsService = A.Fake<ITransactionDetailsService>());
        Services.AddSingleton(_personService = A.Fake<IPersonService>());
        Services.AddSingleton(_localizer = A.Fake<IStringLocalizer<Translation>>());
        Services.AddSingleton(_notificationService = A.Fake<INotificationService>());
        Services.AddSingleton(_operationResultFactory = A.Fake<IOperationResultFactory>());

        A.CallTo(() => _localizer[A<string>._])
            .ReturnsLazily((string key) => new LocalizedString(key, key));

        _persons = [new PersonModel { Id = 1, Name = "John Doe" }];
        A.CallTo(() => _personService.GetAllPersonsAsync()).Returns(_persons);

        Services.AddSingleton(new TransactionDetailsValidator(_localizer));
        Services.AddMudServices();

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<MudPopoverProvider> _popoverProvider = null!;

    private IRenderedComponent<MudDialogProvider> RenderDialog(
        int transactionId = 1,
        int? transactionDetailsId = null,
        int? suggestedDocumentNumber = null)
    {
        _popoverProvider = Render<MudPopoverProvider>();
        var cut = Render<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<TransactionDetailsDialog>
        {
            { x => x.TransactionId, transactionId }
        };
        if (transactionDetailsId.HasValue)
            parameters.Add(x => x.TransactionDetailsId, transactionDetailsId);
        if (suggestedDocumentNumber.HasValue)
            parameters.Add(x => x.SuggestedDocumentNumber, suggestedDocumentNumber);

        cut.InvokeAsync(() =>
            dialogService.ShowAsync<TransactionDetailsDialog>("Dialog", parameters));

        return cut;
    }

    [Test]
    public void AddMode_RendersAndLoadsPersons()
    {
        var cut = RenderDialog();

        A.CallTo(() => _personService.GetAllPersonsAsync()).MustHaveHappenedOnceExactly();
        cut.Markup.Should().Contain("AddEntry");
        cut.Markup.Should().Contain("Cancel");
    }

    [Test]
    public void AddMode_UsesSuggestedDocumentNumber()
    {
        var cut = RenderDialog(suggestedDocumentNumber: 42);

        cut.Markup.Should().Contain("42");
    }

    [Test]
    public void EditMode_LoadsTransactionDetailsAndShowsSaveButton()
    {
        var details = new TransactionDetailsModel
        {
            Id = 10,
            TransactionId = 1,
            DocumentNumber = 100,
            Description = "Test detail",
            Sum = 50m
        };
        A.CallTo(() => _transactionDetailsService.GetTransactionDetailsByIdAsync(10)).Returns(details);

        var cut = RenderDialog(transactionDetailsId: 10);

        A.CallTo(() => _transactionDetailsService.GetTransactionDetailsByIdAsync(10)).MustHaveHappenedOnceExactly();
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
        var details = new TransactionDetailsModel
        {
            Id = 10,
            TransactionId = 1,
            DocumentNumber = 100,
            Description = "Test detail",
            Sum = 50m
        };
        A.CallTo(() => _transactionDetailsService.GetTransactionDetailsByIdAsync(10)).Returns(details);
        A.CallTo(() => _transactionDetailsService.UpdateTransactionDetailsAsync(A<TransactionDetailsModel>._))
            .Returns(new OperationResult { Status = OperationResultStatus.Success });

        var cut = RenderDialog(transactionDetailsId: 10);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _transactionDetailsService.UpdateTransactionDetailsAsync(A<TransactionDetailsModel>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveInEditMode_ShowsNotificationOnFailure()
    {
        var details = new TransactionDetailsModel
        {
            Id = 10,
            TransactionId = 1,
            DocumentNumber = 100,
            Description = "Test detail",
            Sum = 50m
        };
        var failResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Update failed"
        };
        A.CallTo(() => _transactionDetailsService.GetTransactionDetailsByIdAsync(10)).Returns(details);
        A.CallTo(() => _transactionDetailsService.UpdateTransactionDetailsAsync(A<TransactionDetailsModel>._))
            .Returns(failResult);

        var cut = RenderDialog(transactionDetailsId: 10);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _notificationService.ShowOperationResultAsync(failResult)).MustHaveHappened();
    }

    [Test]
    public void EditMode_InputFieldsContainLoadedValues()
    {
        var details = new TransactionDetailsModel
        {
            Id = 10,
            TransactionId = 1,
            DocumentNumber = 100,
            Description = "Test detail",
            Sum = 50m
        };
        A.CallTo(() => _transactionDetailsService.GetTransactionDetailsByIdAsync(10)).Returns(details);

        var cut = RenderDialog(transactionDetailsId: 10);

        var inputs = cut.FindAll("input");
        inputs.Should().Contain(i => i.GetAttribute("value") == "100");
        inputs.Should().Contain(i => i.GetAttribute("value") == "Test detail");
    }

    [Test]
    public async Task AddMode_PersonDropdownContainsItems()
    {
        var cut = RenderDialog();

        var personSelect = cut.FindComponent<MudSelect<int?>>();
        var input = personSelect.Find("input.mud-select-input");
        await cut.InvokeAsync(() => input.MouseDown());

        await _popoverProvider.WaitForAssertionAsync(() =>
            _popoverProvider.FindAll("div.mud-list-item").Count.Should().BeGreaterThan(0));

        var items = _popoverProvider.FindAll("div.mud-list-item").ToArray();
        items.Should().Contain(i => i.TextContent.Contains("John Doe"));
    }

    [Test]
    public async Task AddMode_ValidationPreventsEmptySave()
    {
        var cut = RenderDialog();

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("AddEntry"));
        await cut.InvokeAsync(() => addButton.Click());

        A.CallTo(() => _transactionDetailsService.AddTransactionDetailsAsync(A<TransactionDetailsModel>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task SaveOnSuccess_ClosesDialog()
    {
        var details = new TransactionDetailsModel
        {
            Id = 10,
            TransactionId = 1,
            DocumentNumber = 100,
            Description = "Test detail",
            Sum = 50m
        };
        A.CallTo(() => _transactionDetailsService.GetTransactionDetailsByIdAsync(10)).Returns(details);
        A.CallTo(() => _transactionDetailsService.UpdateTransactionDetailsAsync(A<TransactionDetailsModel>._))
            .Returns(new OperationResult { Status = OperationResultStatus.Success });

        var cut = RenderDialog(transactionDetailsId: 10);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        cut.Markup.Should().NotContain("Save");
    }
}