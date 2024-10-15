namespace BuildingBlocks.Application.Outbox;

public interface IOutbox
{
    void Add(OutboxMessage message);
    Task SaveAsync(CancellationToken cancellationToken = default);
}