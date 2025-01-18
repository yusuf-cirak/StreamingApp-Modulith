using BuildingBlocks.Application.Abstractions.Managers;
using BuildingBlocks.Application.Abstractions.Security;
using BuildingBlocks.Application.Common.Behaviors;
using BuildingBlocks.Application.Common.Services;
using BuildingBlocks.SharedKernel;
using MediatR;
using Xunit;

namespace BuildingBlocks.Application.UnitTests.Common.Behaviors;

public class SensitiveRequestBehaviorTests
{
    private class TestSensitiveRequest : IRequest<Result>, ISensitiveRequest
    {
        public string Data { get; set; } = string.Empty;
    }

    private class TestCurrentUserService : ICurrentUserService
    {
        private readonly string? _userId;

        public TestCurrentUserService(string? userId = null)
        {
            _userId = userId;
        }

        public Option<System.Security.Claims.ClaimsPrincipal> User => throw new NotImplementedException();
        public Option<string> UserId => _userId;
        public bool IsAuthenticated => _userId != null;
    }

    private class TestBlackListManager : IBlackListManager
    {
        private readonly bool _isBlacklisted;

        public TestBlackListManager(bool isBlacklisted = false)
        {
            _isBlacklisted = isBlacklisted;
        }

        public Task<Result> AddToBlackListAsync(string key)
            => Task.FromResult(Result.Success());

        public Task<Result> IsBlackListedAsync(string key)
            => Task.FromResult(_isBlacklisted ? Result.Success() : Result.Failure());

        public Task<Result> RemoveFromBlackListAsync(string key)
            => Task.FromResult(Result.Success());
    }

    [Fact]
    public async Task Handle_WithNonBlacklistedUser_CallsNextDelegate()
    {
        // Arrange
        var currentUserService = new TestCurrentUserService("test-user");
        var blackListManager = new TestBlackListManager(isBlacklisted: false);
        var behavior = new SensitiveRequestBehavior<TestSensitiveRequest, Result>(currentUserService, blackListManager);
        var request = new TestSensitiveRequest();
        var expectedResult = Result.Success();
        RequestHandlerDelegate<Result> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Fact]
    public async Task Handle_WithBlacklistedUser_ReturnsUnauthorized()
    {
        // Arrange
        var currentUserService = new TestCurrentUserService("test-user");
        var blackListManager = new TestBlackListManager(isBlacklisted: true);
        var behavior = new SensitiveRequestBehavior<TestSensitiveRequest, Result>(currentUserService, blackListManager);
        var request = new TestSensitiveRequest();

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(Result.Success()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCache.Unauthorized, result.Error);
    }

    [Fact]
    public async Task Handle_WithNoUserId_ReturnsUnauthorized()
    {
        // Arrange
        var currentUserService = new TestCurrentUserService(null);
        var blackListManager = new TestBlackListManager();
        var behavior = new SensitiveRequestBehavior<TestSensitiveRequest, Result>(currentUserService, blackListManager);
        var request = new TestSensitiveRequest();

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(Result.Success()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCache.Unauthorized, result.Error);
    }
} 