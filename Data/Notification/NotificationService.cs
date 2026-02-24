using MudBlazor;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Notification;

public class NotificationService(ISnackbar snackbar) : INotificationService
{
    public Task ShowOperationResultAsync(IOperationResult result)
    {
        var severity = result.Status switch
        {
            OperationResultStatus.Success => Severity.Success,
            OperationResultStatus.Canceled => Severity.Warning,
            OperationResultStatus.Failed => Severity.Error,
            OperationResultStatus.Warning => Severity.Info,
            _ => Severity.Normal
        };

        var message = result.Message;
        if (result.AffectedItems is > 1)
        {
            message += $" ({result.AffectedItems} items)";
        }
        
        else if (result.Data is int id)
        {
            message += $" (Id: {id})";
        }

        snackbar.Add(message, severity);
        return Task.CompletedTask;
    }
}
