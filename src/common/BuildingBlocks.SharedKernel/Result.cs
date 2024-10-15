namespace BuildingBlocks.SharedKernel;

public interface IResult;

public record Result : IResult
{
    public Error Error { get; } = ErrorCache.None;

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    internal Result(Error error)
    {
        Error = error;
        IsSuccess = false;
    }

    internal Result(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    public static Result Success() => ResultCache.Success;

    public static Result Failure(Error error) => new(error);


    public static Result Failure() => ResultCache.Failure;

    public static implicit operator Result(Error error) => Failure(error);

    public TResult Match<TResult>(Func<TResult> success, Func<Error, TResult> failure)
        => IsSuccess ? success() : failure(Error);
}

public record Result<TValue, TError> : IResult
{
    public TValue Value { get; } = default;
    public TError Error { get; } = default;

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    private Result(TValue value)
    {
        Value = value;
        IsSuccess = true;
    }

    private Result(TError error)
    {
        Error = error;
        IsSuccess = false;
    }

    public static Result<TValue, TError> Success(TValue value) => new(value);

    public static Result<TValue, TError> Failure(TError error) => new(error);

    public static implicit operator Result<TValue, TError>(TValue value) => Success(value);

    public static implicit operator Result<TValue, TError>(TError error) => Failure(error);

    public TResult Match<TResult>(Func<TValue, TResult> success, Func<TError, TResult> failure)
        => this.IsSuccess ? success(this.Value) : failure(this.Error);
}

public static class ResultCache
{
    internal static readonly Result Success = new(true);
    internal static readonly Result Failure = new(false);
    
    public static readonly IResult Unauthorized = Result.Failure(ErrorCache.Unauthorized);
    
    public static readonly IResult BadRequest = Result.Failure(ErrorCache.BadRequest);
    
    public static readonly IResult NotFound = Result.Failure(ErrorCache.NotFound);
    
    public static readonly IResult Forbidden = Result.Failure(ErrorCache.Forbidden);
}

public static class ResultExtensions
{
    public static IResult Create(Error error) => Result.Failure(error);

    public static IResult Create<TValue, TError>(TValue value) => Result<TValue,TError>.Success(value);
}