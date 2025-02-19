using System.Security.Claims;
using BuildingBlocks.Application.Common.Services;
using YC.Monad;

namespace BuildingBlocks.Application.UnitTests.Mocks;


public class MockCurrentUserService : ICurrentUserService
{
    private readonly string? _userId;
    private readonly ClaimsPrincipal? _user;

    public MockCurrentUserService(string? userId = null)
    {
        _userId = userId;
        if (userId != null)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            _user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        }
    }

    public Option<ClaimsPrincipal> User => _user;
    public Option<string> UserId => _userId;
    public bool IsAuthenticated => _userId != null;
}