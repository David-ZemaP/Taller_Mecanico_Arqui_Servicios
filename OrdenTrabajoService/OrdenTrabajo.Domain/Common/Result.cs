namespace Taller_Mecanico_Arqui.Domain.Common
{
    /// <summary>
    /// Represents the outcome of an operation that can either succeed or fail.
    /// Provides a railway-oriented programming pattern for error handling.
    /// </summary>
    public class Result
    {
        protected internal Result(bool isSuccess, string? errorCode, string? errorMessage)
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

        /// <summary>
        /// Creates a failure result with an error code only.
        /// </summary>
        public static Result Failure(string errorCode)
            => new(false, errorCode, null);

        /// <summary>
        /// Executes the appropriate action based on the result state.
        /// </summary>
        public TResult Match<TResult>(Func<TResult> onSuccess, Func<string?, string?, TResult> onFailure)
            => IsSuccess ? onSuccess() : onFailure(ErrorCode, ErrorMessage);

        /// <summary>
        /// Executes the appropriate action based on the result state (void version).
        /// </summary>
        public void Match(Action onSuccess, Action<string?, string?> onFailure)
        {
            if (IsSuccess)
                onSuccess();
            else
                onFailure(ErrorCode, ErrorMessage);
        }

        /// <summary>
        /// Executes an action if the result is a failure.
        /// </summary>
        public Result TapError(Action<string?, string?> action)
        {
            if (IsFailure)
                action(ErrorCode, ErrorMessage);
            return this;
        }

        /// <summary>
        /// Executes an action if the result is a success.
        /// </summary>
        public Result Tap(Action action)
        {
            if (IsSuccess)
                action();
            return this;
        }
    }

    /// <summary>
    /// Represents the outcome of an operation that returns a value or fails.
    /// </summary>
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

        /// <summary>
        /// Creates a failure result with an error code only.
        /// </summary>
        public static new Result<T> Failure(string errorCode)
            => new(false, default, errorCode, null);

        /// <summary>
        /// Maps the success value to another type.
        /// </summary>
        public Result<TNew> Map<TNew>(Func<T?, TNew?> mapFunc)
        {
            if (IsFailure)
                return Result<TNew>.Failure(ErrorCode!, ErrorMessage!);
            
            return Result<TNew>.Success(mapFunc(Value));
        }

        /// <summary>
        /// Binds the result to another Result, flattening nested results.
        /// </summary>
        public Result<TNew> Bind<TNew>(Func<T?, Result<TNew>> bindFunc)
        {
            if (IsFailure)
                return Result<TNew>.Failure(ErrorCode!, ErrorMessage!);
            
            return bindFunc(Value);
        }

        /// <summary>
        /// Executes the appropriate function based on the result state.
        /// </summary>
        public TResult Match<TResult>(Func<T?, TResult> onSuccess, Func<string?, string?, TResult> onFailure)
            => IsSuccess ? onSuccess(Value) : onFailure(ErrorCode, ErrorMessage);

        /// <summary>
        /// Executes the appropriate action based on the result state (void version).
        /// </summary>
        public void Match(Action<T?> onSuccess, Action<string?, string?> onFailure)
        {
            if (IsSuccess)
                onSuccess(Value);
            else
                onFailure(ErrorCode, ErrorMessage);
        }

        /// <summary>
        /// Executes an action if the result is a success, passing the value.
        /// </summary>
        public Result<T> Tap(Action<T?> action)
        {
            if (IsSuccess)
                action(Value);
            return this;
        }

        /// <summary>
        /// Executes an action if the result is a failure.
        /// </summary>
        public new Result<T> TapError(Action<string?, string?> action)
        {
            if (IsFailure)
                action(ErrorCode, ErrorMessage);
            return this;
        }

        /// <summary>
        /// Returns the value or throws an exception if failure.
        /// </summary>
        public T GetValueOrThrow()
        {
            if (IsFailure)
                throw new InvalidOperationException($"Result failed: {ErrorCode} - {ErrorMessage}");
            return Value!;
        }

        /// <summary>
        /// Returns the value or a default if failure.
        /// </summary>
        public T? GetValueOrDefault(T? defaultValue = default)
            => IsSuccess ? Value : defaultValue;

        /// <summary>
        /// Converts to Result of a different type, preserving error state.
        /// </summary>
        public Result<TNew> Cast<TNew>() where TNew : class
        {
            if (IsFailure)
                return Result<TNew>.Failure(ErrorCode!, ErrorMessage!);
            
            return Result<TNew>.Success(Value as TNew);
        }

        /// <summary>
        /// Executes the selector if success, otherwise propagates the failure.
        /// </summary>
        public Result<TNew> SelectMany<TNew>(Func<T?, Result<TNew>> selector)
            => Bind(selector);

        /// <summary>
        /// Projects the success value using the selector.
        /// </summary>
        public Result<TNew> Select<TNew>(Func<T?, TNew?> selector)
            => Map(selector);
    }

    /// <summary>
    /// Represents a collection of errors for scenarios requiring multiple validation failures.
    /// </summary>
    public class ErrorList
    {
        private readonly List<(string Code, string Message)> _errors = new();

        public IReadOnlyList<(string Code, string Message)> Errors => _errors.AsReadOnly();
        public bool HasErrors => _errors.Count > 0;
        public int Count => _errors.Count;

        public void Add(string code, string message)
        {
            _errors.Add((code, message));
        }

        public void AddRange(ErrorList other)
        {
            _errors.AddRange(other.Errors);
        }

        public Result ToResult()
        {
            if (!HasErrors)
                return Result.Success();
            
            var first = _errors[0];
            return Result.Failure(first.Code, string.Join("; ", _errors.Select(e => e.Message)));
        }

        public Result<T> ToResult<T>()
        {
            if (!HasErrors)
                return Result<T>.Success(default);
            
            var first = _errors[0];
            return Result<T>.Failure(first.Code, string.Join("; ", _errors.Select(e => e.Message)));
        }
    }

    /// <summary>
    /// Factory methods for creating results from try-catch blocks.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Wraps a try-catch in a Result, capturing exceptions as failures.
        /// </summary>
        public static Result<T> Try<T>(Func<T> func, string errorCode = "EXCEPTION")
        {
            try
            {
                return Result<T>.Success(func());
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(errorCode, ex.Message);
            }
        }

        /// <summary>
        /// Wraps an async try-catch in a Result, capturing exceptions as failures.
        /// </summary>
        public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func, string errorCode = "EXCEPTION")
        {
            try
            {
                return Result<T>.Success(await func());
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(errorCode, ex.Message);
            }
        }

        /// <summary>
        /// Converts a nullable value to a Result, treating null as a failure.
        /// </summary>
        public static Result<T> ToResult<T>(this T? value, string errorCode, string errorMessage) where T : class
            => value is null ? Result<T>.Failure(errorCode, errorMessage) : Result<T>.Success(value);

        /// <summary>
        /// Converts a nullable value to a Result, treating null as a failure.
        /// </summary>
        public static Result<T> ToResult<T>(this T? value, string errorCode, string errorMessage) where T : struct
            => value.HasValue ? Result<T>.Success(value.Value) : Result<T>.Failure(errorCode, errorMessage);

        /// <summary>
        /// Combines multiple results into a single result with all errors if any fail.
        /// </summary>
        public static Result Combine(this IEnumerable<Result> results)
        {
            var errors = new ErrorList();
            foreach (var result in results)
            {
                if (result.IsFailure)
                    errors.Add(result.ErrorCode!, result.ErrorMessage!);
            }
            return errors.ToResult();
        }

        /// <summary>
        /// Combines multiple results into a single result returning a tuple of values.
        /// </summary>
        public static Result<(T1, T2)> Combine<T1, T2>(Result<T1> r1, Result<T2> r2)
        {
            if (r1.IsFailure)
                return Result<(T1, T2)>.Failure(r1.ErrorCode!, r1.ErrorMessage!);
            if (r2.IsFailure)
                return Result<(T1, T2)>.Failure(r2.ErrorCode!, r2.ErrorMessage!);
            
            return Result<(T1, T2)>.Success((r1.Value!, r2.Value!));
        }

        /// <summary>
        /// Combines multiple results into a single result returning a tuple of values.
        /// </summary>
        public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(Result<T1> r1, Result<T2> r2, Result<T3> r3)
        {
            if (r1.IsFailure)
                return Result<(T1, T2, T3)>.Failure(r1.ErrorCode!, r1.ErrorMessage!);
            if (r2.IsFailure)
                return Result<(T1, T2, T3)>.Failure(r2.ErrorCode!, r2.ErrorMessage!);
            if (r3.IsFailure)
                return Result<(T1, T2, T3)>.Failure(r3.ErrorCode!, r3.ErrorMessage!);
            
            return Result<(T1, T2, T3)>.Success((r1.Value!, r2.Value!, r3.Value!));
        }
    }
}
