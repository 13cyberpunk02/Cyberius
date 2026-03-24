using Cyberius.Domain.Shared;

public sealed class Result<T>
{
    private readonly T?     _value;
    private readonly Error? _error;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    // Добавляем публичный доступ
    public T     Value => IsSuccess  ? _value!  : throw new InvalidOperationException("Result is failure");
    public Error Error => IsFailure  ? _error!  : throw new InvalidOperationException("Result is success");

    private Result(T value)     { _value = value; IsSuccess = true; }
    private Result(Error error) { _error = error; IsSuccess = false; }

    public static Result<T> Success(T value)      => new(value);
    public static Result<T> Failure(Error error)  => new(error);

    public static implicit operator Result<T>(T value)     => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);

    public TResult Match<TResult>(
        Func<T, TResult>     onSuccess,
        Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess(_value!) : onFailure(_error!);
}

public sealed class Result
{
    private readonly Error? _error;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error => IsFailure ? _error! : throw new InvalidOperationException("Result is success");

    private Result()            => IsSuccess = true;
    private Result(Error error) { _error = error; IsSuccess = false; }

    public static Result Success()            => new();
    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => Failure(error);

    public TResult Match<TResult>(
        Func<TResult>        onSuccess,
        Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(_error!);
}