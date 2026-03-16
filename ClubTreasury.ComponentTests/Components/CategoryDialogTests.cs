using Bunit;
using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.Notification;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.ComponentTests.Components;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CategoryDialogTests : BunitContext
{
    private ICategoryService _categoryService = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private INotificationService _notificationService = null!;
    private IResultFactory _resultFactory = null!;

    [SetUp]
    public void SetUp()
    {
        Services.AddSingleton(_categoryService = A.Fake<ICategoryService>());
        Services.AddSingleton(_localizer = A.Fake<IStringLocalizer<Translation>>());
        Services.AddSingleton(_notificationService = A.Fake<INotificationService>());
        Services.AddSingleton(_resultFactory = A.Fake<IResultFactory>());

        A.CallTo(() => _localizer[A<string>._])
            .ReturnsLazily((string key) => new LocalizedString(key, key));

        Services.AddSingleton(new CategoryValidator(_localizer));
        Services.AddMudServices();

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<MudDialogProvider> RenderDialog(int? categoryId = null)
    {
        var cut = Render<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<CategoryDialog>();
        if (categoryId.HasValue)
            parameters.Add(x => x.CategoryId, categoryId);

        cut.InvokeAsync(() =>
            dialogService.ShowAsync<CategoryDialog>("Dialog", parameters));

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
    public void EditMode_LoadsCategoryAndShowsSaveButton()
    {
        var category = new CategoryModel { Id = 1, Name = "Fees" };
        A.CallTo(() => _categoryService.GetCategoryByIdAsync(1)).Returns(category);

        var cut = RenderDialog(categoryId: 1);

        A.CallTo(() => _categoryService.GetCategoryByIdAsync(1)).MustHaveHappenedOnceExactly();
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
        var category = new CategoryModel { Id = 1, Name = "Fees" };
        A.CallTo(() => _categoryService.GetCategoryByIdAsync(1)).Returns(category);
        A.CallTo(() => _categoryService.UpdateCategoryAsync(A<CategoryModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog(categoryId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _categoryService.UpdateCategoryAsync(A<CategoryModel>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveInEditMode_ShowsNotificationOnFailure()
    {
        var category = new CategoryModel { Id = 1, Name = "Fees" };
        var failResult = Result.Failure(new Error("Test.Error", "Update failed"));
        A.CallTo(() => _categoryService.GetCategoryByIdAsync(1)).Returns(category);
        A.CallTo(() => _categoryService.UpdateCategoryAsync(A<CategoryModel>._))
            .Returns(failResult);

        var cut = RenderDialog(categoryId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        A.CallTo(() => _notificationService.ShowResultAsync(failResult)).MustHaveHappened();
    }

    [Test]
    public void EditMode_InputContainsLoadedValue()
    {
        var category = new CategoryModel { Id = 1, Name = "Fees" };
        A.CallTo(() => _categoryService.GetCategoryByIdAsync(1)).Returns(category);

        var cut = RenderDialog(categoryId: 1);

        var input = cut.Find("input");
        input.GetAttribute("value").Should().Be("Fees");
    }

    [Test]
    public async Task AddMode_ValidationPreventsEmptySave()
    {
        var cut = RenderDialog();

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("AddEntry"));
        await cut.InvokeAsync(() => addButton.Click());

        A.CallTo(() => _categoryService.AddCategoryAsync(A<CategoryModel>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task AddMode_CallsAddOnSuccess()
    {
        A.CallTo(() => _categoryService.AddCategoryAsync(A<CategoryModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog();

        var input = cut.Find("input");
        input.Input("New Category");

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("AddEntry"));
        await cut.InvokeAsync(() => addButton.Click());

        A.CallTo(() => _categoryService.AddCategoryAsync(
            A<CategoryModel>.That.Matches(m => m.Name == "New Category")))
            .MustHaveHappened();
    }

    [Test]
    public async Task SaveOnSuccess_ClosesDialog()
    {
        var category = new CategoryModel { Id = 1, Name = "Fees" };
        A.CallTo(() => _categoryService.GetCategoryByIdAsync(1)).Returns(category);
        A.CallTo(() => _categoryService.UpdateCategoryAsync(A<CategoryModel>._))
            .Returns(Result.Success());

        var cut = RenderDialog(categoryId: 1);

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveButton.Click());

        cut.Markup.Should().NotContain("Save");
    }
}
