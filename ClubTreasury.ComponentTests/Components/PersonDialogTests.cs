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

namespace ClubTreasury.ComponentTests.Components;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class PersonDialogTests : BunitContext
{
    private IPersonService _personService = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private INotificationService _notificationService = null!;
    private IResultFactory _resultFactory = null!;

    [SetUp]
    public void SetUp()
    {
        Services.AddSingleton(_personService = A.Fake<IPersonService>());
        Services.AddSingleton(_localizer = A.Fake<IStringLocalizer<Translation>>());
        Services.AddSingleton(_notificationService = A.Fake<INotificationService>());
        Services.AddSingleton(_resultFactory = A.Fake<IResultFactory>());

        A.CallTo(() => _localizer[A<string>._])
            .ReturnsLazily((string key) => new LocalizedString(key, key));

        Services.AddSingleton(new PersonValidator(_localizer));
        Services.AddMudServices();

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<MudDialogProvider> RenderDialog(int? personId = null)
    {
        var cut = Render<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<PersonDialog>();
        if (personId.HasValue)
            parameters.Add(x => x.PersonId, personId);

        cut.InvokeAsync(() =>
            dialogService.ShowAsync<PersonDialog>("Dialog", parameters));

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
    public void EditMode_LoadsPersonAndShowsSaveButton()
    {
        var person = new PersonModel { Id = 1, Name = "John Doe" };
        A.CallTo(() => _personService.GetPersonByIdAsync(1)).Returns(person);

        var cut = RenderDialog(personId: 1);

        A.CallTo(() => _personService.GetPersonByIdAsync(1)).MustHaveHappenedOnceExactly();
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
        var person = new PersonModel { Id = 1, Name = "John Doe" };
        A.CallTo(() => _personService.GetPersonByIdAsync(1)).Returns(person);
        A.CallTo(() => _personService.UpdatePersonAsync(A<PersonModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog(personId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _personService.UpdatePersonAsync(A<PersonModel>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveInEditMode_ShowsNotificationOnFailure()
    {
        var person = new PersonModel { Id = 1, Name = "John Doe" };
        var failResult = Result.Failure(new Error("Test.Error", "Update failed"));
        A.CallTo(() => _personService.GetPersonByIdAsync(1)).Returns(person);
        A.CallTo(() => _personService.UpdatePersonAsync(A<PersonModel>._))
            .Returns(failResult);

        var cut = RenderDialog(personId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _notificationService.ShowResultAsync(failResult)).MustHaveHappened();
    }

    [Test]
    public void EditMode_InputContainsLoadedValue()
    {
        var person = new PersonModel { Id = 1, Name = "John Doe" };
        A.CallTo(() => _personService.GetPersonByIdAsync(1)).Returns(person);

        var cut = RenderDialog(personId: 1);

        var input = cut.Find("input");
        input.GetAttribute("value").Should().Be("John Doe");
    }

    [Test]
    public async Task AddMode_ValidationPreventsEmptySave()
    {
        var cut = RenderDialog();

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("AddEntry"));
        await cut.InvokeAsync(() => addButton.Click());

        A.CallTo(() => _personService.AddPersonAsync(A<PersonModel>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task AddMode_CallsAddOnSuccess()
    {
        A.CallTo(() => _personService.AddPersonAsync(A<PersonModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog();

        var input = cut.Find("input");
        input.Input("Jane Doe");

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("AddEntry"));
        await cut.InvokeAsync(() => addButton.Click());

        A.CallTo(() => _personService.AddPersonAsync(
            A<PersonModel>.That.Matches(m => m.Name == "Jane Doe")))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveOnSuccess_ClosesDialog()
    {
        var person = new PersonModel { Id = 1, Name = "John Doe" };
        A.CallTo(() => _personService.GetPersonByIdAsync(1)).Returns(person);
        A.CallTo(() => _personService.UpdatePersonAsync(A<PersonModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog(personId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        cut.Markup.Should().NotContain("Save");
    }
}
