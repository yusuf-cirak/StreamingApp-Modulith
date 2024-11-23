using BuildingBlocks.Application.Abstractions.Security;
using BuildingBlocks.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse>(IHttpContextAccessor httpContextAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ISecuredRequest
    where TResponse : Result
{

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var claims = httpContextAccessor.HttpContext?.User.Claims.ToList();

        if (claims is null or { Count: 0 })
        {
            return (TResponse)ResultCache.Unauthorized;
        }

        return await next();
    }
}