namespace LexiLink.BuildingBlocks.Domain;

public abstract class TypedIdValueBase : IEquatable<TypedIdValueBase>
{
    public Guid Value {get; }

    protected TypedIdValueBase(Guid value)
    {
        if(value == Guid.Empty)
            throw new InvalidOperationException("Guid.Empty değeri geçersizdir.");

        Value = value;
    }

    public override bool Equals(object? obj)
    {
        if(ReferenceEquals(null, obj)) return false;
        
        return obj is TypedIdValueBase other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();   
    }

    public bool Equals(TypedIdValueBase? other)
    {
        return Value == other?.Value;
    }

    public static bool operator ==(TypedIdValueBase? left, TypedIdValueBase? right)
    {
        if(object.Equals(left, null))
        {
            return object.Equals(right, null);
        }
        return left.Equals(right);
    }

    public static bool operator !=(TypedIdValueBase? left, TypedIdValueBase? right)
    {
        return !(left == right);
    }
}