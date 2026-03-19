namespace ClubTreasury.Data.OperationResult;

public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error Canceled = new("Operation.Canceled", string.Empty);
}

public enum ErrorType
{
    Failure,
    Warning
}
