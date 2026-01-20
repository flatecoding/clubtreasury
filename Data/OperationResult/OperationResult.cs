namespace TTCCashRegister.Data.OperationResult;


public class OperationResult : IOperationResult
{
    public OperationResultStatus Status { get; init; }
    public string Message { get; init; } = "";
    public object? Data { get; init; }
    public int? AffectedItems { get; init; }
    
}