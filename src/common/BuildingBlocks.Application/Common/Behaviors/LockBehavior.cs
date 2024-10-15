using BuildingBlocks.Application.Abstractions.Caching;
using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.SharedKernel;
using MediatR;

namespace BuildingBlocks.Application.Common.Behaviors;

public sealed class LockBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ILockRequest
    where TResponse : IResult, new()
{
    private readonly ICacheService _cacheService;

    public LockBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var lockAcquired = await _cacheService.TakeLockAsync(request.Key, TimeSpan.FromSeconds(request.Expiration));
        try
        {
            if (!lockAcquired)
            {

                return (TResponse)ResultCache.Forbidden;
            }

            return await next();
        }
        finally
        {
            if (lockAcquired && request.ReleaseImmediately)
            {
                await _cacheService.ReleaseLockAsync(request.Key);
            }
        }
    }
}