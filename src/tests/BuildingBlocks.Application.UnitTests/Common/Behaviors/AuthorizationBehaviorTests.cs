using BuildingBlocks.Application.Abstractions.Security;
using BuildingBlocks.Application.Common.Behaviors;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using BuildingBlocks.Application.Common.Services;
using Xunit;

namespace BuildingBlocks.Application.UnitTests.Common.Behaviors;

public class AuthorizationBehaviorTests
{
    private class TestSecuredRequest : IRequest<IResult>, ISecuredRequest
    {
        public string Data { get; set; } = string.Empty;
    }

    [Fact]
    public async Task Handle_WithAuthenticatedUser_CallsNextDelegate()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "test-user")
                }, "test"))
            }
        };
        
        var currentUserService = new CurrentUserService(httpContextAccessor);

        var behavior = new AuthorizationBehavior<TestSecuredRequest, IResult>(currentUserService);
        var request = new TestSecuredRequest();
        var expectedResult = Results.Ok();
        RequestHandlerDelegate<IResult> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Fact]
    public async Task Handle_WithNoHttpContext_ReturnsUnauthorized()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor();
        
        var currentUserService = new CurrentUserService(httpContextAccessor);

        var behavior = new AuthorizationBehavior<TestSecuredRequest, IResult>(currentUserService);
        var request = new TestSecuredRequest();

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(Results.Ok()), CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task Handle_WithNoClaims_ReturnsUnauthorized()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        var currentUserService = new CurrentUserService(httpContextAccessor);

        var behavior = new AuthorizationBehavior<TestSecuredRequest, IResult>(currentUserService);
        var request = new TestSecuredRequest();

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(Results.Ok()), CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }
} 