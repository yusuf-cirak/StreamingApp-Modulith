namespace BuildingBlocks.Domain;

public abstract record ValueObject 
{
    public abstract IEnumerable<object> GetEqualityComponents();

    public virtual bool Equals(ValueObject other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType() && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode() => GetEqualityComponents()
        .Select(x => x != null ? x.GetHashCode() : 0)
        .Aggregate((x, y) => x ^ y);
}