using BuildingBlocks.Application.Abstractions.Caching;
using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.Application.Common.Behaviors;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace BuildingBlocks.Application.UnitTests.Common.Behaviors;

public class LockBehaviorTests
{
    private class TestLockRequest : IRequest<IResult>, ILockRequest
    {
        public string Key { get; set; } = "test-key";
        public int Expiration { get; set; } = 30;
        public bool ReleaseImmediately { get; set; } = true;
    }

    private class TestCacheService : ICacheService
    {
        private readonly bool _lockAcquired;
        private bool _lockReleased;

        public TestCacheService(bool lockAcquired = true)
        {
            _lockAcquired = lockAcquired;
            _lockReleased = false;
        }

        public bool WasLockReleased => _lockReleased;

        public Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiresIn = null,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<T> GetOrUseFactoryAsync<T>(string key, Func<Task<T>> factory,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<bool> RemoveAsync(string key)
            => throw new NotImplementedException();

        public Task<bool> ReleaseLockAsync(string key)
        {
            _lockReleased = true;
            return Task.FromResult(true);
        }

        public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiresIn = null,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<bool> TakeLockAsync(string key, TimeSpan expiration)
            => Task.FromResult(_lockAcquired);
    }

    [Fact]
    public async Task Handle_WhenLockAcquired_CallsNextDelegate()
    {
        // Arrange
        var cacheService = new TestCacheService(lockAcquired: true);
        var behavior = new LockBehavior<TestLockRequest, IResult>(cacheService);
        var request = new TestLockRequest();
        var expectedResult = Results.Ok();
        RequestHandlerDelegate<IResult> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Same(expectedResult, result);
        Assert.True(cacheService.WasLockReleased);
    }

    [Fact]
    public async Task Handle_WhenLockNotAcquired_ReturnsForbidden()
    {
        // Arrange
        var cacheService = new TestCacheService(lockAcquired: false);
        var behavior = new LockBehavior<TestLockRequest, IResult>(cacheService);
        var request = new TestLockRequest();

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(Results.Ok()), CancellationToken.None);

        // Assert
        Assert.IsType<ForbidHttpResult>(result);
        Assert.False(cacheService.WasLockReleased);
    }

    [Fact]
    public async Task Handle_WhenReleaseImmediatelyFalse_DoesNotReleaseLock()
    {
        // Arrange
        var cacheService = new TestCacheService(lockAcquired: true);
        var behavior = new LockBehavior<TestLockRequest, IResult>(cacheService);
        var request = new TestLockRequest { ReleaseImmediately = false };
        var expectedResult = Results.Ok();
        RequestHandlerDelegate<IResult> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Same(expectedResult, result);
        Assert.False(cacheService.WasLockReleased);
    }

    [Fact]
    public async Task Handle_WhenNextDelegateThrows_ReleasesLockAndThrowsException()
    {
        // Arrange
        var cacheService = new TestCacheService(lockAcquired: true);
        var behavior = new LockBehavior<TestLockRequest, IResult>(cacheService);
        var request = new TestLockRequest();
        RequestHandlerDelegate<IResult> next = () => throw new Exception("Test exception");

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => behavior.Handle(request, next, CancellationToken.None));
        Assert.True(cacheService.WasLockReleased);
    }
} 