using MudBlazor;

namespace TTCCashRegister.Data.Source;

public interface INotificationService
{
    Task ShowDialogResultAsync(DialogResult? result);
}