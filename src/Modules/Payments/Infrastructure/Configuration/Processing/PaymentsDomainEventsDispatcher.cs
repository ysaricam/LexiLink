using Autofac;
using Autofac.Core;
using LexiLink.Common.Application.Events;
using LexiLink.Common.Application.Outbox;
using LexiLink.Common.Domain;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;
using LexiLink.Common.Infrastructure.Serialization;
using LexiLink.Modules.Payments.Infrastructure.Outbox;
using MediatR;
using Newtonsoft.Json;

namespace LexiLink.Modules.Payments.Infrastructure.Configuration.Processing;

internal class PaymentsDomainEventsDispatcher
{
    private readonly PaymentsContext _context;
    private readonly IMediator _mediator;
    private readonly ILifetimeScope _scope;
    private readonly OutboxAccessor _outbox;
    private readonly IDomainNotificationsMapper _domainNotificationsMapper;

    internal PaymentsDomainEventsDispatcher(
        PaymentsContext context,
        IMediator mediator,
        ILifetimeScope scope,
        OutboxAccessor outbox,
        IDomainNotificationsMapper domainNotificationsMapper)
    {
        _context = context;
        _mediator = mediator;
        _scope = scope;
        _outbox = outbox;
        _domainNotificationsMapper = domainNotificationsMapper;
    }

    public async Task DispatchEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = _context.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        List<IDomainEventNotification<IDomainEvent>> domainEventNotifications = [];
        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(IDomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var domainNotification = _scope.ResolveOptional(notificationType, new List<Parameter>
            {
                new NamedParameter("domainEvent", domainEvent),
                new NamedParameter("id", domainEvent.Id)
            });

            if (domainNotification is not null)
            {
                domainEventNotifications.Add((IDomainEventNotification<IDomainEvent>)domainNotification);
            }
        }

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
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

            _outbox.Add(new OutboxMessage(
                domainNotification.Id,
                domainNotification.DomainEvent.OccurredOn,
                type,
                data));
        }
    }
}
