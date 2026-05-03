namespace LexiLink.Common.Domain;

public abstract class TypedIdValueBase : IEquatable<TypedIdValueBase>
{
    public Guid Value { get; }

    protected TypedIdValueBase(Guid value)
    {
        if (value == Guid.Empty)
            throw new InvalidOperationException("Id value cannot be empty.");
        Value = value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TypedIdValueBase other)
            return false;

        return GetType() == other.GetType() && Value == other.Value;
    }

    public bool Equals(TypedIdValueBase? other)
    {
        if (other is null)
            return false;

        return GetType() == other.GetType() && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Value);
    }

    public static bool operator ==(TypedIdValueBase? left, TypedIdValueBase? right)
    {
        if (left is null)
            return right is null;

        return left.Equals(right);
    }

    public static bool operator !=(TypedIdValueBase? left, TypedIdValueBase? right)
    {
        return !(left == right);
    }
}