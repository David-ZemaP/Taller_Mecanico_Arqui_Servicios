namespace Taller_Mecanico_Users.Domain.Common
{
    public class Result
    {
        protected Result(bool isSuccess, string? errorCode, string? errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? ErrorCode { get; }
        public string? ErrorMessage { get; }

        public static Result Success() => new(true, null, null);

        public static Result Failure(string errorCode, string errorMessage)
            => new(false, errorCode, errorMessage);
    }

    public class Result<T> : Result
    {
        private Result(bool isSuccess, T? value, string? errorCode, string? errorMessage)
            : base(isSuccess, errorCode, errorMessage)
        {
            Value = value;
        }

        public T? Value { get; }

        public static Result<T> Success(T? value)
            => new(true, value, null, null);

        public static new Result<T> Failure(string errorCode, string errorMessage)
            => new(false, default, errorCode, errorMessage);
    }
}
