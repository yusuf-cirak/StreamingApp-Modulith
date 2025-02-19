using BuildingBlocks.Application.Abstractions.Caching;

namespace BuildingBlocks.Application.UnitTests.Mocks;

public class MockCacheService : ICacheService
{
    private readonly bool _lockAcquired;

    public MockCacheService(bool lockAcquired)
    {
        _lockAcquired = lockAcquired;
    }

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

    public Task<bool> ReleaseLockAsync(string key) => Task.FromResult(true);

    public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiresIn = null,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> TakeLockAsync(string key, TimeSpan expiration) => Task.FromResult(_lockAcquired);
}