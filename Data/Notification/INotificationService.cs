using MudBlazor;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Notification;

public interface INotificationService
{
    [Obsolete("Use ShowOperationResultAsync instead. This method will be removed in a future version.")]
    Task ShowDialogResultAsync(DialogResult? result);
    Task ShowOperationResultAsync(IOperationResult result);
}