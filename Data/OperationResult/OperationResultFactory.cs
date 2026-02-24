using Microsoft.Extensions.Localization;

namespace ClubTreasury.Data.OperationResult;

public class OperationResultFactory(IStringLocalizer<Translation> localizer) : IOperationResultFactory
{
    public OperationResult Canceled()
        => new()
        {
            Status = OperationResultStatus.Canceled,
            Message = localizer["OperationCancelled"]
        };
    
    public OperationResult Success()
        => new()
        {
            Status = OperationResultStatus.Success,
            Message = localizer["OperationSuccessful"]
        };
    
    public OperationResult SuccessAdded(string entityName, object? data = null)
        => new()
        {
            Status = OperationResultStatus.Success,
            Message = $"{entityName} {localizer["AddedSuccessfully"]}",
            Data = data
        };
    
    public OperationResult SuccessUpdated(string entityName, object? data = null)
        => new()
        {
            Status = OperationResultStatus.Success,
            Message = $"{entityName} {localizer["UpdatedSuccessfully"]}",
            Data = data
        };
    
    public OperationResult SuccessDeleted(string entityName, object? data = null)
        => new()
        {
            Status = OperationResultStatus.Success,
            Message = $"{entityName} {localizer["DeletedSuccessfully"]}",
            Data = data
        };
    
    public OperationResult FailedToAdd(string entityName, string? details = null)
        => new()
        {
            Status = OperationResultStatus.Failed,
            Message = $"{localizer["FailedToAdd"]} {entityName}" + (details != null ? $": {details}" : "")
        };
    
    public OperationResult DialogIsEmpty(string entityName, string? details = null)
        => new()
        {
            Status = OperationResultStatus.Warning,
            Message = $"{localizer["RequiredDataIsMissing"]} {entityName}" + (details != null ? $": {details}" : "")
        };
    
    public OperationResult FailedToUpdate(string entityName, string? details = null)
        => new()
        {
            Status = OperationResultStatus.Failed,
            Message = $"{localizer["FailedToUpdate"]} {entityName}" + (details != null ? $": {details}" : "")
        };
    
    public OperationResult FailedToDelete(string entityName, string? details = null)
        => new()
        {
            Status = OperationResultStatus.Failed,
            Message = $"{localizer["FailedToDelete"]} {entityName}" + (details != null ? $": {details}" : "")
        };
    
    public OperationResult NotFound(string entityName, object id)
        => new()
        {
            Status = OperationResultStatus.Failed,
            Message = $"{entityName} {localizer["NotFound"]} (Id: {id})"
        };
    
    public OperationResult AlreadyExists(string entityName, string? details = null)
        => new()
        {
            Status = OperationResultStatus.Warning,
            Message = $"{entityName} {localizer["AlreadyExists"]}" + (details != null ? $": {details}" : "!")
        };
    
    public OperationResult ExportSuccessful(string fileName)
        => new()
        {
            Status = OperationResultStatus.Success,
            Message = $"{localizer["ExportSuccessful"]}: {fileName}"
        };

    public OperationResult ExportFailed(string? details = null)
        => new()
        {
            Status = OperationResultStatus.Failed,
            Message = localizer["ExportFailed"] + (details != null ? $": {details}" : "")
        };
    
    public OperationResult DateRangeInvalid(string? details = null)
        => new()
        {
            Status = OperationResultStatus.Warning,
            Message = localizer["DateWarning"] + (details != null ? $": {details}" : "")
        };
    
    public OperationResult ImportSuccessful(string fileName)
        => new()
        {
            Status = OperationResultStatus.Success,
            Message = $"{localizer["ImportSuccessful"]}: {fileName}"
        };

    public OperationResult ImportFailed(string? details = null)
        => new()
        {
            Status = OperationResultStatus.Failed,
            Message = localizer["ImportFailed"] + (details != null ? $": {details}" : "")
        };
}