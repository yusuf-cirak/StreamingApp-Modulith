using System.Security.Claims;
using BuildingBlocks.Application.Common.Extensions;
using BuildingBlocks.SharedKernel;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Application.Common.Services;

public interface ICurrentUserService
{
    public Option<ClaimsPrincipal> User { get; }

    public Option<string> UserId { get; }

    public bool IsAuthenticated { get; }
}

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Option<ClaimsPrincipal> User { get; } = httpContextAccessor.HttpContext?.User;
    public Option<string> UserId { get; } = httpContextAccessor.HttpContext?.User?.GetUserId() ?? Option<string>.None();
    public bool IsAuthenticated { get; } = httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}