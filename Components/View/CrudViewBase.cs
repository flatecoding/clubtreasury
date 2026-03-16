using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using ClubTreasury.Data.DialogService;
using ClubTreasury.Data.Notification;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Components.View;

public abstract class CrudViewBase<TModel, TDialog> : ComponentBase
    where TModel : class, new()
    where TDialog : ComponentBase
{
    [Inject] protected IDialogService DialogService { get; set; } = null!;
    [Inject] protected INotificationService NotificationService { get; set; } = null!;
    [Inject] protected IStringLocalizer<Translation> Localizer { get; set; } = null!;

    protected List<TModel>? AllEntries;
    protected string SearchTerm = string.Empty;
    protected TModel SelectedItem = new();

    protected abstract Task<List<TModel>> LoadAllAsync();
    protected abstract string EntityIdParameterName { get; }
    protected abstract string EntityLocalizedName { get; }
    protected abstract int GetEntityId(TModel entity);
    protected abstract string GetEntityDisplayName(TModel entity);
    protected abstract Task<Result> DeleteEntityAsync(int id);
    protected abstract bool MatchesFilter(TModel? element, string searchTerm);

    protected virtual DialogOptions? CustomDialogOptions => null;

    protected override async Task OnInitializedAsync()
    {
        AllEntries = await LoadAllAsync();
    }

    protected bool FilterFunc(TModel? element) => MatchesFilter(element, SearchTerm);

    protected async Task ShowAddDialog()
    {
        var result = await DialogService.ShowDialogWithNotificationAsync<TDialog>(
            NotificationService, options: CustomDialogOptions);
        if (result is { IsSuccess: true })
            await UpdateView();
    }

    protected async Task ShowEditDialog(int entityId)
    {
        var result = await DialogService.ShowDialogWithNotificationAsync<TDialog>(
            NotificationService,
            $"{Localizer["Edit"]}",
            new DialogParameters { [EntityIdParameterName] = entityId },
            CustomDialogOptions);
        if (result is { IsSuccess: true })
            await UpdateView();
    }

    protected async Task ShowDeleteDialog(TModel entity)
    {
        await DialogService.ShowConfirmDeleteDialogAsync(
            NotificationService,
            entityName: EntityLocalizedName,
            itemName: GetEntityDisplayName(entity),
            onConfirm: () => DeleteEntityAsync(GetEntityId(entity)),
            onSuccess: UpdateView,
            deleteTitle: Localizer["Delete"]);
    }

    private async Task UpdateView()
    {
        AllEntries = await LoadAllAsync();
        await InvokeAsync(StateHasChanged);
    }
}
