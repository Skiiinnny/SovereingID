namespace SovereignID.Auth.Domain;

/// <summary>
/// Represents success/failure with typed values.
/// </summary>
public sealed record Result<TValue, TError>
{
    private Result(bool isSuccess, TValue? value, TError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public TValue? Value { get; }
    public TError? Error { get; }

    public static Result<TValue, TError> Success(TValue value) => new(true, value, default);
    public static Result<TValue, TError> Failure(TError error) => new(false, default, error);
}

/// <summary>
/// Represents a successful operation with no payload.
/// </summary>
public readonly record struct Unit
{
    public static Unit Value => default;
}
