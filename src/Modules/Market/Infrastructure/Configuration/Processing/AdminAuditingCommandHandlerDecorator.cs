using LexiLink.Common.Application.Admin;
using LexiLink.Common.Application.Outbox;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;
using LexiLink.Modules.Market.Application.Configuration.Commands;
using LexiLink.Modules.Market.Application.Contracts;
using LexiLink.Modules.Market.Infrastructure.Outbox;
using LexiLink.Modules.Market.Infrastructure.Outbox.DomainEventNotifications;
using Newtonsoft.Json;

namespace LexiLink.Modules.Market.Infrastructure.Configuration.Processing;

internal sealed class AdminAuditingCommandHandlerDecorator<T> : ICommandHandler<T>
    where T : ICommand
{
    private readonly ICommandHandler<T> _decorated;
    private readonly IAdminAuthorizationContext _adminContext;
    private readonly OutboxAccessor _outbox;
    private readonly IClock _clock;
    private readonly IDomainNotificationsMapper _notificationsMapper;

    public AdminAuditingCommandHandlerDecorator(
        ICommandHandler<T> decorated,
        IAdminAuthorizationContext adminContext,
        OutboxAccessor outbox,
        IClock clock,
        IDomainNotificationsMapper notificationsMapper)
    {
        _decorated = decorated;
        _adminContext = adminContext;
        _outbox = outbox;
        _clock = clock;
        _notificationsMapper = notificationsMapper;
    }

    public async Task Handle(T command, CancellationToken cancellationToken)
    {
        if (command is not IAdminCommand adminCommand)
        {
            await _decorated.Handle(command, cancellationToken);
            return;
        }

        var adminUserId = _adminContext.RequireAdminUserId();
        await _decorated.Handle(command, cancellationToken);

        MarketAdminAuditing.EnqueueAudit(command, adminCommand, adminUserId, _clock, _outbox, _notificationsMapper);
    }
}

internal sealed class AdminAuditingCommandHandlerWithResultDecorator<T, TResult> : ICommandHandler<T, TResult>
    where T : ICommand<TResult>
{
    private readonly ICommandHandler<T, TResult> _decorated;
    private readonly IAdminAuthorizationContext _adminContext;
    private readonly OutboxAccessor _outbox;
    private readonly IClock _clock;
    private readonly IDomainNotificationsMapper _notificationsMapper;

    public AdminAuditingCommandHandlerWithResultDecorator(
        ICommandHandler<T, TResult> decorated,
        IAdminAuthorizationContext adminContext,
        OutboxAccessor outbox,
        IClock clock,
        IDomainNotificationsMapper notificationsMapper)
    {
        _decorated = decorated;
        _adminContext = adminContext;
        _outbox = outbox;
        _clock = clock;
        _notificationsMapper = notificationsMapper;
    }

    public async Task<TResult> Handle(T command, CancellationToken cancellationToken)
    {
        if (command is not IAdminCommand adminCommand)
        {
            return await _decorated.Handle(command, cancellationToken);
        }

        var adminUserId = _adminContext.RequireAdminUserId();
        var result = await _decorated.Handle(command, cancellationToken);

        MarketAdminAuditing.EnqueueAudit(command, adminCommand, adminUserId, _clock, _outbox, _notificationsMapper);

        return result;
    }
}

internal static class MarketAdminAuditing
{
    public static void EnqueueAudit(
        object command,
        IAdminCommand adminCommand,
        Guid adminUserId,
        IClock clock,
        OutboxAccessor outbox,
        IDomainNotificationsMapper notificationsMapper)
    {
        var notification = new MarketAdminActionPerformedNotification(
            id: Guid.NewGuid(),
            occurredOn: clock.UtcNow,
            adminUserId: adminUserId,
            actionType: command.GetType().Name,
            targetType: adminCommand.AuditTargetType,
            targetId: adminCommand.AuditTargetId,
            payloadJson: JsonConvert.SerializeObject(command));

        var typeKey = notificationsMapper.GetName(typeof(MarketAdminActionPerformedNotification))
            ?? throw new InvalidOperationException(
                "MarketAdminActionPerformedNotification is not registered in DomainNotificationsMap. " +
                "Add the mapping in MarketStartup.");

        outbox.Add(new OutboxMessage(
            notification.Id,
            notification.OccurredOn,
            typeKey,
            JsonConvert.SerializeObject(notification)));
    }
}
