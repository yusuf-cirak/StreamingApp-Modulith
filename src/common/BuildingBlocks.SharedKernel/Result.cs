using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.SharedKernel;

public record Result
{
    public Error Error { get; protected init; } = ErrorCache.None;

    public bool IsSuccess { get; protected init; }

    public bool IsFailure => !IsSuccess;

    protected Result()
    {
    }

    private Result(Error error)
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

public record Result<TValue> : Result
{
    public TValue Value { get; } = default!;

    private Result(TValue value)
    {
        Value = value;
        IsSuccess = true;
    }

    private Result(Error error) : base()
    {
        Error = error;
        IsSuccess = false;
    }

    public static Result<TValue> Success(TValue value) => new(value);

    public new static Result<TValue> Failure(Error error) => new(error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator Result<TValue>(Error error) => Failure(error);

    public static Result<TValue> From(Result result) => result.IsSuccess ? Success(default!) : Failure(result.Error);

    public TResult Match<TResult>(Func<TValue, TResult> success, Func<Error, TResult> failure)
        => this.IsSuccess ? success(this.Value) : failure(this.Error);
}

public static class ResultCache
{
    internal static readonly Result Success = new(true);
    internal static readonly Result Failure = new(false);

    public static readonly Result Unauthorized = Result.Failure(ErrorCache.Unauthorized);

    public static readonly Result BadRequest = Result.Failure(ErrorCache.BadRequest);

    public static readonly Result NotFound = Result.Failure(ErrorCache.NotFound);

    public static readonly Result Forbidden = Result.Failure(ErrorCache.Forbidden);
}

public static class ResultExtensions
{
    public static Result Create(Error error) => Result.Failure(error);

    public static Result<TValue> Create<TValue>(TValue value) => Result<TValue>.Success(value);

    public static TResponse ToTypedResult<TResponse>(this Result result) where TResponse : Result
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)result;
        }

        var genericType = typeof(TResponse).GetGenericArguments()[0];
        var genericResultType = typeof(Result<>).MakeGenericType(genericType);
        return (TResponse)genericResultType.GetMethod(nameof(Result<object>.From))!
            .Invoke(null, [result])!;
    }
}

public static partial class HttpResultExtensions
{
    // todo: do this in request flow. this place is not good.
    public static IResult ToHttpResponse<TValue>(this Result<TValue> result, int statusCode = StatusCodes.Status200OK)
    {
        return result.Match(
            (value) => statusCode switch
            {
                200 => Results.Ok(value),
                201 => Results.Created(string.Empty, value),
                204 => Results.NoContent(),
                _ => Results.StatusCode(statusCode)
            },
            (error) => error.Status switch
            {
                400 => Results.BadRequest(error),
                401 => Results.Unauthorized(),
                403 => Results.Forbid(),
                404 => Results.NotFound(error),
                409 => Results.Conflict(error),
                500 => Results.Problem(title: error.Code, detail: error.Message, statusCode: 500),
                _ => Results.BadRequest(error)
            });
    }

    public static IResult ToHttpResponse(this Result result, int statusCode = StatusCodes.Status200OK)
    {
        return result.Match(
            () => statusCode switch
            {
                200 => Results.Ok(),
                201 => Results.Created(),
                204 => Results.NoContent(),
                _ => Results.StatusCode(statusCode)
            },
            (failure) => failure.Status switch
            {
                400 => Results.BadRequest(failure),
                401 => Results.Unauthorized(),
                403 => Results.Forbid(),
                404 => Results.NotFound(failure),
                409 => Results.Conflict(failure),
                500 => Results.Problem(title: failure.Code, detail: failure.Message, statusCode: 500),
                _ => Results.BadRequest(failure)
            });
    }
}