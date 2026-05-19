using System.Collections;
using System.Reflection;
using LexiLink.Common.Domain;

namespace LexiLink.Modules.Administration.Tests.SeedWork;

internal static class DomainEventsTestHelper
{
    public static IReadOnlyCollection<IDomainEvent> GetAllDomainEvents(Entity aggregate)
    {
        var events = new List<IDomainEvent>();
        Collect(aggregate, events, []);
        return events;
    }

    public static void ClearAllDomainEvents(Entity aggregate) => Walk(aggregate, e => e.ClearDomainEvents(), []);

    private static void Collect(Entity entity, List<IDomainEvent> sink, HashSet<object> visited)
    {
        if (!visited.Add(entity)) return;
        sink.AddRange(entity.DomainEvents);

        foreach (var field in EnumerateBackingFields(entity))
        {
            switch (field)
            {
                case Entity nested:
                    Collect(nested, sink, visited);
                    break;
                case IEnumerable enumerable:
                    foreach (var item in enumerable)
                        if (item is Entity nestedItem)
                            Collect(nestedItem, sink, visited);
                    break;
            }
        }
    }

    private static void Walk(Entity entity, Action<Entity> action, HashSet<object> visited)
    {
        if (!visited.Add(entity)) return;
        action(entity);

        foreach (var field in EnumerateBackingFields(entity))
        {
            switch (field)
            {
                case Entity nested:
                    Walk(nested, action, visited);
                    break;
                case IEnumerable enumerable:
                    foreach (var item in enumerable)
                        if (item is Entity nestedItem)
                            Walk(nestedItem, action, visited);
                    break;
            }
        }
    }

    private static IEnumerable<object> EnumerateBackingFields(Entity entity)
    {
        var type = entity.GetType();
        while (type is not null && type != typeof(object))
        {
            foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var value = f.GetValue(entity);
                if (value is not null) yield return value;
            }
            type = type.BaseType;
        }
    }
}
