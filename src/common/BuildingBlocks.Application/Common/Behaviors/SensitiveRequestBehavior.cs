using BuildingBlocks.Application.Abstractions.Security;
using BuildingBlocks.Application.Common.Services;
using BuildingBlocks.SharedKernel;
using MediatR;
using BuildingBlocks.Application.Abstractions.Managers;

namespace BuildingBlocks.Application.Common.Behaviors;

public sealed class SensitiveRequestBehavior<TRequest, TResponse>(
    ICurrentUserService currentUserService,
    IBlackListManager blacklistManager)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ISensitiveRequest
    where TResponse : Result
{
    private static TResponse CreateUnauthorizedResult()
    {
        return ResultCache.Unauthorized.ToTypedResult<TResponse>();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            return CreateUnauthorizedResult();
        }

        var blacklistResult = await blacklistManager.IsBlackListedAsync(currentUserService.UserId.GetValueOrFail());
        
        if (blacklistResult.IsSuccess)
        {
            return CreateUnauthorizedResult();
        }

        return await next();
    }
}