using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using ClubTreasury.Data.Notification;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Components.Dialog;

public abstract class SimpleEntityDialogBase<TModel> : ComponentBase where TModel : new()
{
    [Inject] protected IStringLocalizer<Translation> Localizer { get; set; } = null!;
    [Inject] protected INotificationService NotificationService { get; set; } = null!;
    [Inject] protected IResultFactory ResultFactory { get; set; } = null!;

    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = null!;

    protected MudForm Form = new();
    protected TModel Model = new();
    protected bool IsEditMode;

    protected abstract int? EntityId { get; }
    protected abstract Func<object, string, Task<IEnumerable<string>>> ValidateValue { get; }
    protected abstract Task<TModel?> LoadEntityAsync(int id);
    protected abstract Task<Result> AddEntityAsync(TModel model);
    protected abstract Task<Result> UpdateEntityAsync(TModel model);

    protected override async Task OnInitializedAsync()
    {
        IsEditMode = EntityId.HasValue;
        await MudDialog.SetTitleAsync(IsEditMode ? Localizer["Edit"] : Localizer["AddEntry"]);
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (!IsEditMode)
        {
            Model = new TModel();
            return;
        }

        Model = await LoadEntityAsync(EntityId!.Value) ?? new TModel();
    }

    protected async Task Save()
    {
        await Form.Validate();
        if (!Form.IsValid)
            return;

        var result = IsEditMode
            ? await UpdateEntityAsync(Model)
            : await AddEntityAsync(Model);

        if (result.IsSuccess)
            MudDialog.Close(DialogResult.Ok(result));
        else
            await NotificationService.ShowResultAsync(result);
    }

    protected void Cancel() => MudDialog.Close(DialogResult.Ok(ResultFactory.Canceled()));
}
