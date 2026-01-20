namespace TTCCashRegister.Data.OperationResult;

public interface IOperationResult
{
    OperationResultStatus Status { get; }
    string Message { get; }
    object? Data { get; }
    int? AffectedItems { get; }
}