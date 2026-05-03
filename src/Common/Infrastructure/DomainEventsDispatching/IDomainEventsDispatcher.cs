namespace LexiLink.Common.Infrastructure.DomainEventsDispatching;

public interface IDomainEventsDispatcher
{
    Task DispatchEventsAsync();
}