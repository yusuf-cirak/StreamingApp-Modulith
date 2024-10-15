using BuildingBlocks.Application.Abstractions.Managers;
using BuildingBlocks.Application.Abstractions.Security;
using BuildingBlocks.Application.Common.Services;
using BuildingBlocks.SharedKernel;
using MediatR;

namespace BuildingBlocks.Application.Common.Behaviors;

public sealed class SensitiveRequestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ISensitiveRequest
    where TResponse : IResult , new()
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IBlackListManager _blacklistManager;


    public SensitiveRequestBehavior(ICurrentUserService currentUserService, IBlackListManager blacklistManager)
    {
        _currentUserService = currentUserService;
        _blacklistManager = blacklistManager;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId.ToString();

        var blacklistResult = await _blacklistManager.IsBlackListedAsync(userId);

        if (blacklistResult.IsSuccess)
        {
            var response = ResultCache.Unauthorized;

            return (TResponse)response;
        }

        return await next();
    }
}