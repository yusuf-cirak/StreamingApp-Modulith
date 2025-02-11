using System.Security.Claims;
using BuildingBlocks.Application.Common.Extensions;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Application.Common.Services;

public interface ICurrentUserService
{
    public Option<ClaimsPrincipal> User { get; }

    public Option<string> UserId { get; }

    public bool IsAuthenticated { get; }
}

public sealed class CurrentUserService : ICurrentUserService
{
    public Option<ClaimsPrincipal> User { get; }

    public Option<string> UserId { get; }

    public bool IsAuthenticated => UserId.HasValue;


    public CurrentUserService(IHttpContextAccessor ctxAccessor)
    {
        User = ctxAccessor.HttpContext?.User;
        UserId = User.Bind(u => u.GetUserId());
    }
}