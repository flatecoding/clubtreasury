using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Notification;

public interface INotificationService
{
    Task ShowResultAsync(Result result);
}
