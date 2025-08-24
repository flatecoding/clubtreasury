namespace TTCCashRegister.Data.Source;

using MudBlazor;

public class NotificationService(ISnackbar snackbar)
{
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
}
