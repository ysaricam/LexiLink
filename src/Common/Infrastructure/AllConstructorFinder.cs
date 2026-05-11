using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core.Activators.Reflection;

namespace LexiLink.Common.Infrastructure;

public class AllConstructorFinder : IConstructorFinder
{
    private static readonly ConcurrentDictionary<Type, ConstructorInfo[]> Cache = new();

    public ConstructorInfo[] FindConstructors(Type targetType)
    {
        var constructors = Cache.GetOrAdd(targetType, t =>
            t.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

        return constructors.Length > 0
            ? constructors
            : throw new NoConstructorsFoundException(targetType, this);
    }
}
