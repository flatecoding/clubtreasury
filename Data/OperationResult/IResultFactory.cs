namespace ClubTreasury.Data.OperationResult;

public interface IResultFactory
{
    Result Canceled();
    Result Success();
    Result SuccessAdded(string entityName, object? data = null);
    Result SuccessUpdated(string entityName, object? data = null);
    Result SuccessDeleted(string entityName, object? data = null);
    Result FailedToAdd(string entityName, string? details = null);
    Result DialogIsEmpty(string entityName, string? details = null);
    Result FailedToUpdate(string entityName, string? details = null);
    Result FailedToDelete(string entityName, string? details = null);
    Result NotFound(string entityName, object id);
    Result AlreadyExists(string entityName, string? details = null);
    Result ExportSuccessful(string fileName);
    Result ExportFailed(string? details = null);
    Result ImportSuccessful(string fileName);
    Result ImportFailed(string? details = null);
    Result DateRangeInvalid(string? details = null);
}
