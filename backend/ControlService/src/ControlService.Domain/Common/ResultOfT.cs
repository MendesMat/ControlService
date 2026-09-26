using System.Diagnostics.CodeAnalysis;

namespace ControlService.Domain.Common;

[SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "Result<T>.Success/Failure factories are the API shape decided in ADR-0009.")]
public sealed class Result<T>
{
    private readonly T? _value;

    private Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("A failed result has no value.");

    public static Result<T> Success(T value) => new(true, value, error: null);

    public static Result<T> Failure(Error error) => new(false, default, error);
}
