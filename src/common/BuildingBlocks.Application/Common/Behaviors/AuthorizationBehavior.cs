using BuildingBlocks.Application.Abstractions.Security;
using BuildingBlocks.Application.Common.Services;
using BuildingBlocks.SharedKernel;
using MediatR;

namespace BuildingBlocks.Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse>(ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ISecuredRequest
    where TResponse : Result
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            var result = ResultCache.Unauthorized.ToTypedResult<TResponse>();
            return Task.FromResult(result);
        }

        return next();
    }
}