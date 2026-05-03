using System.Collections.Concurrent;
using System.Reflection;

namespace LexiLink.Common.Domain;

public abstract class ValueObject : IEquatable<ValueObject>
{
    private static readonly ConcurrentDictionary<Type, (List<PropertyInfo> Props, List<FieldInfo> Fields)>
        _cache = new();

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);

    public bool Equals(ValueObject? other) => Equals((object?)other);

    public override bool Equals(object? obj)
    {
        if (obj is null || GetType() != obj.GetType()) return false;

        var (props, fields) = GetMembers();
        return props.All(p => Equals(p.GetValue(this), p.GetValue(obj)))
            && fields.All(f => Equals(f.GetValue(this), f.GetValue(obj)));
    }

    public override int GetHashCode()
    {
        var (props, fields) = GetMembers();
        unchecked
        {
            int hash = 17;
            foreach (var prop in props)
                hash = (hash * 23) + (prop.GetValue(this)?.GetHashCode() ?? 0);
            foreach (var field in fields)
                hash = (hash * 23) + (field.GetValue(this)?.GetHashCode() ?? 0);
            return hash;
        }
    }

    protected static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);
    }

    private (List<PropertyInfo>, List<FieldInfo>) GetMembers()
    {
        return _cache.GetOrAdd(GetType(), t =>
        {
            var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetCustomAttribute<IgnoreMemberAttribute>() == null)
                .ToList();

            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.GetCustomAttribute<IgnoreMemberAttribute>() == null)
                .ToList();

            return (props, fields);
        });
    }
}
