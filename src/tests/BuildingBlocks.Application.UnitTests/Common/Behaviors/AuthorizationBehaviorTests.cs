using BuildingBlocks.Application.Abstractions.Security;
using BuildingBlocks.Application.Common.Behaviors;
using BuildingBlocks.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using BuildingBlocks.Application.Common.Services;
using Xunit;

namespace BuildingBlocks.Application.UnitTests.Common.Behaviors;

public class AuthorizationBehaviorTests
{
    private class TestSecuredRequest : IRequest<Result>, ISecuredRequest
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

        var behavior = new AuthorizationBehavior<TestSecuredRequest, Result>(currentUserService);
        var request = new TestSecuredRequest();
        var expectedResult = Result.Success();
        RequestHandlerDelegate<Result> next = () => Task.FromResult(expectedResult);

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

        var behavior = new AuthorizationBehavior<TestSecuredRequest, Result>(currentUserService);
        var request = new TestSecuredRequest();

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(Result.Success()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCache.Unauthorized, result.Error);
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

        var behavior = new AuthorizationBehavior<TestSecuredRequest, Result>(currentUserService);
        var request = new TestSecuredRequest();

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(Result.Success()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCache.Unauthorized, result.Error);
    }
} 