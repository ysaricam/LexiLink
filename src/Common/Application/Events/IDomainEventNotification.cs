using LexiLink.Common.Domain;
using MediatR;

namespace LexiLink.Common.Application.Events;

public interface IDomainEventNotification<out TEventType> : IDomainEventNotification
    where TEventType : IDomainEvent
{
    TEventType DomainEvent { get; }
}

public interface IDomainEventNotification : INotification
{
    Guid Id { get; }
}
