namespace BuildingBlocks.Domain;

public abstract class AggregateRoot<TId>(TId id) : BaseEntity<TId>(id)
    where TId : notnull;