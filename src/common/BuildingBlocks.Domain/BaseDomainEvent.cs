namespace BuildingBlocks.Domain;

public class BaseDomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateVersion7();

    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}