using BuildingBlocks.Application.Abstractions.Managers;
using YC.Monad;

namespace BuildingBlocks.Application.UnitTests.Mocks;

public class MockBlackListManager : IBlackListManager
{
    private readonly bool _isBlacklisted;

    public MockBlackListManager(bool isBlacklisted)
    {
        _isBlacklisted = isBlacklisted;
    }

    public Task<Result> AddToBlackListAsync(string key) => Task.FromResult<Result>(Result.Success());

    public Task<Result> IsBlackListedAsync(string key) =>
        Task.FromResult(_isBlacklisted ? Result.Success() : Result.Failure());

    public Task<Result> RemoveFromBlackListAsync(string key) => Task.FromResult<Result>(Result.Success());
}