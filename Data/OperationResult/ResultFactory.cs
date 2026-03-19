using Microsoft.Extensions.Localization;

namespace ClubTreasury.Data.OperationResult;

public class ResultFactory(IStringLocalizer<Translation> localizer) : IResultFactory
{
    public Result Canceled()
        => Result.Failure(Error.Canceled with { Message = localizer["OperationCancelled"] });

    public Result Success()
        => Result.Success(localizer["OperationSuccessful"]);

    public Result SuccessAdded(string entityName, object? data = null)
    {
        var message = $"{entityName} {localizer["AddedSuccessfully"]}";
        if (data is int id) message += $" (Id: {id})";
        return Result.Success(message);
    }

    public Result SuccessUpdated(string entityName, object? data = null)
    {
        var message = $"{entityName} {localizer["UpdatedSuccessfully"]}";
        if (data is int id) message += $" (Id: {id})";
        return Result.Success(message);
    }

    public Result SuccessDeleted(string entityName, object? data = null)
    {
        var message = $"{entityName} {localizer["DeletedSuccessfully"]}";
        if (data is int id) message += $" (Id: {id})";
        return Result.Success(message);
    }

    public Result FailedToAdd(string entityName, string? details = null)
        => Result.Failure(new Error(
            "Entity.FailedToAdd",
            $"{localizer["FailedToAdd"]} {entityName}" + FormatDetails(details)));

    public Result DialogIsEmpty(string entityName, string? details = null)
        => Result.Failure(new Error(
            "Validation.DialogIsEmpty",
            $"{localizer["RequiredDataIsMissing"]} {entityName}" + FormatDetails(details),
            ErrorType.Warning));

    public Result FailedToUpdate(string entityName, string? details = null)
        => Result.Failure(new Error(
            "Entity.FailedToUpdate",
            $"{localizer["FailedToUpdate"]} {entityName}" + FormatDetails(details)));

    public Result FailedToDelete(string entityName, string? details = null)
        => Result.Failure(new Error(
            "Entity.FailedToDelete",
            $"{localizer["FailedToDelete"]} {entityName}" + FormatDetails(details)));

    public Result NotFound(string entityName, object id)
        => Result.Failure(new Error(
            "Entity.NotFound",
            $"{entityName} {localizer["NotFound"]} (Id: {id})"));

    public Result AlreadyExists(string entityName, string? details = null)
        => Result.Failure(new Error(
            "Entity.AlreadyExists",
            $"{entityName} {localizer["AlreadyExists"]}" + (details != null ? $": {details}" : "!"),
            ErrorType.Warning));

    public Result ExportSuccessful(string fileName)
        => Result.Success($"{localizer["ExportSuccessful"]}: {fileName}");

    public Result ExportFailed(string? details = null)
        => Result.Failure(new Error(
            "Export.Failed",
            localizer["ExportFailed"] + FormatDetails(details)));

    public Result DateRangeInvalid(string? details = null)
        => Result.Failure(new Error(
            "Validation.DateRangeInvalid",
            localizer["DateWarning"] + FormatDetails(details),
            ErrorType.Warning));

    public Result ImportSuccessful(string fileName)
        => Result.Success($"{localizer["ImportSuccessful"]}: {fileName}");

    public Result ImportFailed(string? details = null)
        => Result.Failure(new Error(
            "Import.Failed",
            localizer["ImportFailed"] + FormatDetails(details)));

    private static string FormatDetails(string? details)
        => details != null ? $": {details}" : "";
}
