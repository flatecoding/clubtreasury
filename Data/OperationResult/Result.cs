namespace ClubTreasury.Data.OperationResult;

public class Result
{
    protected Result(bool isSuccess, Error error, string message)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
            throw new ArgumentException("Invalid error", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
        Message = message;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public string Message { get; }

    public static Result Success(string message = "") => new(true, Error.None, message);
    public static Result Failure(Error error) => new(false, error, error.Message);

    public static implicit operator Result(Error error) => Failure(error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value, string message) : base(true, Error.None, message) => _value = value;
    private Result(Error error) : base(false, error, error.Message) { }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on failure result");

    public static Result<T> Success(T value, string message = "") => new(value, message);
    public new static Result<T> Failure(Error error) => new(error);
}
