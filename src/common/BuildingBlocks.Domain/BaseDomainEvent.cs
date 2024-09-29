namespace BuildingBlocks.Domain;

public class BaseDomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}