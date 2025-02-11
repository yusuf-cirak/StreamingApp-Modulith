

namespace BuildingBlocks.Application.Abstractions.Managers;

public interface IBlackListManager
{
    Task<Result> IsBlackListedAsync(string key);
    Task<Result> AddToBlackListAsync(string key);
    Task<Result> RemoveFromBlackListAsync(string key);
}