using BuildingBlocks.Application.Abstractions.Managers;
using System.Collections.Concurrent;
using YC.Monad;

namespace BuildingBlocks.Application.UnitTests.Managers;

public class BlackListManagerTests
{
    private class TestBlackListManager : IBlackListManager
    {
        private readonly ConcurrentDictionary<string, bool> _blackList = new();

        public Task<Result> AddToBlackListAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Task.FromResult(Result.Failure(Error.Create("BlackList.InvalidKey", "Key cannot be empty")));

            _blackList.TryAdd(key, true);
            return Task.FromResult(Result.Success());
        }

        public Task<Result> IsBlackListedAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Task.FromResult(Result.Failure(Error.Create("BlackList.InvalidKey", "Key cannot be empty")));

            var isBlacklisted = _blackList.ContainsKey(key);
            return Task.FromResult(isBlacklisted ? Result.Success() : Result.Failure(Error.Create("BlackList.NotFound", "Key not found in blacklist")));
        }

        public Task<Result> RemoveFromBlackListAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Task.FromResult(Result.Failure(Error.Create("BlackList.InvalidKey", "Key cannot be empty")));

            _blackList.TryRemove(key, out _);
            return Task.FromResult(Result.Success());
        }
    }

    private readonly IBlackListManager _blackListManager;

    public BlackListManagerTests()
    {
        _blackListManager = new TestBlackListManager();
    }

    [Fact]
    public async Task AddToBlackList_WithValidKey_Succeeds()
    {
        // Arrange
        var key = "test-key";

        // Act
        var result = await _blackListManager.AddToBlackListAsync(key);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddToBlackList_WithEmptyKey_Fails()
    {
        // Arrange
        var key = "";

        // Act
        var result = await _blackListManager.AddToBlackListAsync(key);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("BlackList.InvalidKey", result.Error.Code);
    }

    [Fact]
    public async Task IsBlackListed_WithExistingKey_ReturnsSuccess()
    {
        // Arrange
        var key = "test-key";
        await _blackListManager.AddToBlackListAsync(key);

        // Act
        var result = await _blackListManager.IsBlackListedAsync(key);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task IsBlackListed_WithNonExistingKey_ReturnsFailure()
    {
        // Arrange
        var key = "non-existing-key";

        // Act
        var result = await _blackListManager.IsBlackListedAsync(key);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("BlackList.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task RemoveFromBlackList_WithExistingKey_Succeeds()
    {
        // Arrange
        var key = "test-key";
        await _blackListManager.AddToBlackListAsync(key);

        // Act
        var removeResult = await _blackListManager.RemoveFromBlackListAsync(key);
        var checkResult = await _blackListManager.IsBlackListedAsync(key);

        // Assert
        Assert.True(removeResult.IsSuccess);
        Assert.True(checkResult.IsFailure);
    }

    [Fact]
    public async Task RemoveFromBlackList_WithEmptyKey_Fails()
    {
        // Arrange
        var key = "";

        // Act
        var result = await _blackListManager.RemoveFromBlackListAsync(key);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("BlackList.InvalidKey", result.Error.Code);
    }
} 