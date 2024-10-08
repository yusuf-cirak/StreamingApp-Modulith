namespace BuildingBlocks.Domain;

public abstract class BaseEntity<TId>(TId id)
    where TId : notnull
{
    public TId Id { get; protected set; } = id;
    
    private List<IDomainEvent> _domainEvents;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents?.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents?.Clear();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents ??= [];

        this._domainEvents.Add(domainEvent);
    }

    public override bool Equals(object obj)
    {
        return obj is BaseEntity<TId> entity && this.Id.Equals(entity.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
}