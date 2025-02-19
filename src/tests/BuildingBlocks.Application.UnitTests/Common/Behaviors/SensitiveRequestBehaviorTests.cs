using BuildingBlocks.Application.Abstractions.Managers;
using BuildingBlocks.Application.Abstractions.Security;
using BuildingBlocks.Application.Common.Behaviors;
using BuildingBlocks.Application.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using BuildingBlocks.Application.UnitTests.Mocks;
using Xunit;
using YC.Monad;

namespace BuildingBlocks.Application.UnitTests.Common.Behaviors;

public class SensitiveRequestBehaviorTests
{
    private class TestSensitiveRequest : IRequest<IResult>, ISensitiveRequest
    {
        public string Data { get; set; } = string.Empty;
    }

    [Fact]
    public async Task Handle_WithNonBlacklistedUser_CallsNextDelegate()
    {
        // Arrange
        var currentUserService = new MockCurrentUserService("test-user");
        var blackListManager = new MockBlackListManager(isBlacklisted: false);
        var behavior =
            new SensitiveRequestBehavior<TestSensitiveRequest, IResult>(currentUserService, blackListManager);
        var request = new TestSensitiveRequest();
        var expectedResult = Results.Ok();
        RequestHandlerDelegate<IResult> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Fact]
    public async Task Handle_WithBlacklistedUser_ReturnsForbid()
    {
        // Arrange
        var currentUserService = new MockCurrentUserService("test-user");
        var blackListManager = new MockBlackListManager(isBlacklisted: true);
        var behavior =
            new SensitiveRequestBehavior<TestSensitiveRequest, IResult>(currentUserService, blackListManager);
        var request = new TestSensitiveRequest();

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(Results.Ok()), CancellationToken.None);

        // Assert
        Assert.IsType<ForbidHttpResult>(result);
    }

    [Fact]
    public async Task Handle_WithNoUserId_ReturnsUnauthorized()
    {
        // Arrange
        var currentUserService = new MockCurrentUserService(null);
        var blackListManager = new MockBlackListManager(true);
        var behavior =
            new SensitiveRequestBehavior<TestSensitiveRequest, IResult>(currentUserService, blackListManager);
        var request = new TestSensitiveRequest();

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(Results.Ok()), CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }
}