using MudBlazor;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Notification;

public class NotificationService(ISnackbar snackbar) : INotificationService
{
    [Obsolete("Use ShowOperationResultAsync instead. This method will be removed in a future version.")]
    public Task ShowDialogResultAsync(DialogResult? result)
    {
        if (result?.Data is true)
        {
            snackbar.Add("The transaction was successful", Severity.Success);
        }
        else if (result?.Canceled == true)
        {
            snackbar.Add("The transaction has been cancelled", Severity.Warning);
        }
        else
        {
            snackbar.Add("An error has occurred during transaction", Severity.Error);
        }
        return Task.CompletedTask;
    }

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
