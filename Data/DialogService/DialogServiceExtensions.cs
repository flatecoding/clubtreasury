using Microsoft.AspNetCore.Components;
using MudBlazor;
using ClubTreasury.Components.Dialog;
using ClubTreasury.Data.Notification;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.DialogService;

public static class DialogServiceExtensions
{
    private static readonly DialogOptions DefaultDialogOptions = new()
    {
        MaxWidth = MaxWidth.Medium,
        FullWidth = true,
        BackdropClick = false,
        CloseButton = false
    };

    public static async Task<Result?> ShowDialogWithNotificationAsync<T>(
        this IDialogService dialogService,
        INotificationService notificationService,
        string title = "",
        DialogParameters? parameters = null,
        DialogOptions? options = null) where T : ComponentBase
    {
        parameters ??= new DialogParameters();
        var mergedOptions = MergeOptions(options);
        var dialog = await dialogService.ShowAsync<T>(title, parameters, options: mergedOptions);
        var result = await dialog.Result;

        if (result?.Data is not Result operationResult) return null;
        await notificationService.ShowResultAsync(operationResult);
        return operationResult;
    }

    public static async Task ShowConfirmDeleteDialogAsync(
        this IDialogService dialogService,
        INotificationService notificationService,
        string entityName,
        string itemName,
        Func<Task<Result>> onConfirm,
        Func<Task>? onSuccess = null,
        string deleteTitle = "Delete",
        DialogOptions? options = null)
    {
        var parameters = new DialogParameters
        {
            ["EntityName"] = entityName,
            ["ItemName"] = itemName,
            ["OnConfirm"] = onConfirm
        };

        var mergedOptions = MergeOptions(options);

        var result = await dialogService.ShowDialogWithNotificationAsync<ConfirmDeleteDialog>(
            notificationService,
            title: deleteTitle,
            parameters: parameters,
            options: mergedOptions);

        if (result is { IsSuccess: true } && onSuccess != null)
        {
            await onSuccess();
        }
    }

    private static DialogOptions MergeOptions(DialogOptions? overrides)
    {
        if (overrides is null)
            return DefaultDialogOptions;

        return new DialogOptions
        {
            MaxWidth = overrides.MaxWidth ?? DefaultDialogOptions.MaxWidth,
            FullWidth = overrides.FullWidth ?? DefaultDialogOptions.FullWidth,
            BackdropClick = overrides.BackdropClick ?? DefaultDialogOptions.BackdropClick,
            CloseButton = overrides.CloseButton ?? DefaultDialogOptions.CloseButton,
            NoHeader = overrides.NoHeader ?? DefaultDialogOptions.NoHeader,
            Position = overrides.Position ?? DefaultDialogOptions.Position,
        };
    }
}
