namespace Taller_Mecanico_Arqui.Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    protected Result(bool isSuccess, string? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string errorCode, string errorMessage) => new(false, errorCode, errorMessage);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    private Result(bool isSuccess, T? value, string? errorCode, string? errorMessage)
        : base(isSuccess, errorCode, errorMessage)
    {
        _value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null, null);
    public static new Result<T> Failure(string errorCode, string errorMessage) => new(false, default, errorCode, errorMessage);
}
