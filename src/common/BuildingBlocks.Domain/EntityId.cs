namespace BuildingBlocks.Domain;

public abstract record EntityId<TId>(TId Value) : ValueObject
    where TId : notnull
{
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}


public sealed record GuidEntityId(Guid Value) : EntityId<Guid>(Value)
{
    public static GuidEntityId Create() => new(Guid.NewGuid());
}