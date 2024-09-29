namespace BuildingBlocks.Domain;

public abstract class BaseEntity
{
    private List<IDomainEvent> _domainEvents;

    /// <summary>
    /// Domain events occurred.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents?.AsReadOnly();

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    /// <summary>
    /// Add domain event.
    /// </summary>
    /// <param name="domainEvent">Domain event.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents ??= [];

        this._domainEvents.Add(domainEvent);
    }
}