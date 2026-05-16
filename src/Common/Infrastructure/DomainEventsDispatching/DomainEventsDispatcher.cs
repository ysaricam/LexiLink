using Autofac;
using Autofac.Core;
using LexiLink.Common.Application.Events;
using LexiLink.Common.Application.Outbox;
using LexiLink.Common.Domain;
using LexiLink.Common.Infrastructure.Serialization;
using MediatR;
using Newtonsoft.Json;

namespace LexiLink.Common.Infrastructure.DomainEventsDispatching;

public class DomainEventsDispatcher : IDomainEventsDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILifetimeScope _scope;
    private readonly IOutbox _outbox;
    private readonly IDomainEventsAccessor _domainEventsProvider;
    private readonly IDomainNotificationsMapper _domainNotificationsMapper;

    public DomainEventsDispatcher(
        IMediator mediator,
        ILifetimeScope scope,
        IOutbox outbox,
        IDomainEventsAccessor domainEventsProvider,
        IDomainNotificationsMapper domainNotificationsMapper)
    {
        _mediator = mediator;
        _scope = scope;
        _outbox = outbox;
        _domainEventsProvider = domainEventsProvider;
        _domainNotificationsMapper = domainNotificationsMapper;
    }

    public async Task DispatchEventsAsync()
    {
        var domainEvents = _domainEventsProvider.GetAllDomainEvents();

        List<IDomainEventNotification<IDomainEvent>> domainEventNotifications = [];
        foreach(var domainEvent in domainEvents)
        {
            Type domainEventNotificationType = typeof(IDomainEventNotification<>);
            var domainNotifiationWithGenericType = domainEventNotificationType.MakeGenericType(domainEvent.GetType());
            var domainNotification = _scope.ResolveOptional(domainNotifiationWithGenericType, new List<Parameter>
            {
                new NamedParameter("domainEvent", domainEvent),
                new NamedParameter("id", domainEvent.Id)
            });

            if(domainNotification != null)
                domainEventNotifications.Add((IDomainEventNotification<IDomainEvent>)domainNotification);
        }

        _domainEventsProvider.ClearAllDomainEvents();

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent);
        }

        foreach (var domainNotification in domainEventNotifications)
        {
            var type = _domainNotificationsMapper.GetName(domainNotification.GetType());
            if (type is null)
            {
                throw new ApplicationException(
                    $"Domain notification type '{domainNotification.GetType().FullName}' is not mapped.");
            }

            var data = JsonConvert.SerializeObject(domainNotification, new JsonSerializerSettings
            {
                ContractResolver = new AllPropertiesContractResolver()
            });

            var outboxMessage = new OutboxMessage(
                domainNotification.Id,
                domainNotification.DomainEvent.OccurredOn,
                type,
                data);

            _outbox.Add(outboxMessage);
        }

    }
}
