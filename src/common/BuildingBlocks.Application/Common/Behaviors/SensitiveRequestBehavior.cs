using BuildingBlocks.Application.Abstractions.Security;
using BuildingBlocks.Application.Common.Services;
using MediatR;
using BuildingBlocks.Application.Abstractions.Managers;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Application.Common.Behaviors;

public sealed class SensitiveRequestBehavior<TRequest, TResponse>(
    ICurrentUserService currentUserService,
    IBlackListManager blacklistManager)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ISensitiveRequest
    where TResponse : IResult
{
    private static readonly TResponse UnauthorizedResult = (TResponse)Results.Unauthorized();
    private static readonly TResponse ForbiddenResult = (TResponse)Results.Forbid();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            return UnauthorizedResult;
        }

        var blacklistResult = await blacklistManager.IsBlackListedAsync(currentUserService.UserId.GetValueOrFail());
        
        if (blacklistResult.IsSuccess)
        {
            return ForbiddenResult;
        }

        return await next();
    }
}