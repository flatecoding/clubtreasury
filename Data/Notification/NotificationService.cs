using MudBlazor;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Notification;

public class NotificationService(ISnackbar snackbar) : INotificationService
{
    public Task ShowResultAsync(Result result)
    {
        var severity = result switch
        {
            { IsSuccess: true } => Severity.Success,
            { Error.Code: "Operation.Canceled" } => Severity.Warning,
            { Error.Type: ErrorType.Warning } => Severity.Info,
            _ => Severity.Error
        };

        snackbar.Add(result.Message, severity);
        return Task.CompletedTask;
    }
}
