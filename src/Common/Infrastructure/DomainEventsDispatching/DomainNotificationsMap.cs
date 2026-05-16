namespace LexiLink.Common.Infrastructure.DomainEventsDispatching;

public static class DomainNotificationsMap
{
    public static readonly BiDictionary<string, Type> Instance = new();
}
