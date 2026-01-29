namespace TTCCashRegister.Data.OperationResult;

public interface IOperationResultFactory
{
    OperationResult Canceled();
    OperationResult Success();
    OperationResult SuccessAdded(string entityName, object? data = null);
    OperationResult SuccessUpdated(string entityName, object? data = null);
    OperationResult SuccessDeleted(string entityName, object? data = null);
    OperationResult FailedToAdd(string entityName, string? details = null);
    OperationResult DialogIsEmpty(string entityName, string? details = null);
    OperationResult FailedToUpdate(string entityName, string? details = null);
    OperationResult FailedToDelete(string entityName, string? details = null);
    OperationResult NotFound(string entityName, object id);
    OperationResult AlreadyExists(string entityName, string? details = null);
    OperationResult ExportSuccessful(string fileName);
    OperationResult ExportFailed(string? details = null);
    OperationResult ImportSuccessful(string fileName);
    OperationResult ImportFailed(string? details = null);
    public OperationResult DateRangeInvalid(string? details = null);
}