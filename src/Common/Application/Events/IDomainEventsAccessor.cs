using LexiLink.Common.Domain;

namespace LexiLink.Common.Application.Events;

public interface IDomainEventsAccessor
{
    IReadOnlyCollection<IDomainEvent> GetAllDomainEvents();
    void ClearAllDomainEvents();
}
