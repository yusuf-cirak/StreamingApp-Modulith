using BuildingBlocks.Application.Abstractions.Security;
using BuildingBlocks.Application.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse>(ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ISecuredRequest
    where TResponse : IResult
{
    private static readonly Task<TResponse> UnauthorizedResult = Task.FromResult((TResponse)Results.Unauthorized());
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            return UnauthorizedResult;
        }

        return next();
    }
}