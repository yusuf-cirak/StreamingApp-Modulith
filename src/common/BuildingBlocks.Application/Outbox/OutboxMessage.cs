namespace BuildingBlocks.Application.Outbox;

public sealed record OutboxMessage(string Type, string Data)
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Type { get; } = Type;
    public string Data { get; } = Data;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public DateTime? ProcessedDate { get; private set; }

    public void MarkAsProcessed() => ProcessedDate = DateTime.UtcNow;
}