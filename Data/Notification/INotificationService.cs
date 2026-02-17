using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Notification;

public interface INotificationService
{
    Task ShowOperationResultAsync(IOperationResult result);
}