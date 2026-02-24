using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Notification;

public interface INotificationService
{
    Task ShowOperationResultAsync(IOperationResult result);
}