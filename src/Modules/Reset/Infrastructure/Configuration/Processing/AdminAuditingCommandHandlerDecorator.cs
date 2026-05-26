using LexiLink.Common.Application.Admin;
using LexiLink.Common.Application.Outbox;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;
using LexiLink.Modules.Reset.Application.Configuration.Commands;
using LexiLink.Modules.Reset.Application.Contracts;
using LexiLink.Modules.Reset.Infrastructure.Outbox;
using LexiLink.Modules.Reset.Infrastructure.Outbox.DomainEventNotifications;
using Newtonsoft.Json;

namespace LexiLink.Modules.Reset.Infrastructure.Configuration.Processing;

/// <summary>
/// Reset-private copy of the Quests B7 audit decorator template
/// (per-module Kamil rule). Captures IAdminCommand executions, enforces
/// admin authorization, and enqueues an outbox audit notification that
/// publishes <c>AdminActionPerformedIntegrationEvent</c>.
/// </summary>
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

        ResetAdminAuditing.EnqueueAudit(command, adminCommand, adminUserId, _clock, _outbox, _notificationsMapper);
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

        ResetAdminAuditing.EnqueueAudit(command, adminCommand, adminUserId, _clock, _outbox, _notificationsMapper);

        return result;
    }
}

internal static class ResetAdminAuditing
{
    public static void EnqueueAudit(
        object command,
        IAdminCommand adminCommand,
        Guid adminUserId,
        IClock clock,
        OutboxAccessor outbox,
        IDomainNotificationsMapper notificationsMapper)
    {
        var notification = new ResetAdminActionPerformedNotification(
            id: Guid.NewGuid(),
            occurredOn: clock.UtcNow,
            adminUserId: adminUserId,
            actionType: command.GetType().Name,
            targetType: adminCommand.AuditTargetType,
            targetId: adminCommand.AuditTargetId,
            payloadJson: JsonConvert.SerializeObject(command));

        var typeKey = notificationsMapper.GetName(typeof(ResetAdminActionPerformedNotification))
            ?? throw new InvalidOperationException(
                "ResetAdminActionPerformedNotification is not registered in DomainNotificationsMap. " +
                "Add the mapping in ResetStartup.");

        outbox.Add(new OutboxMessage(
            notification.Id,
            notification.OccurredOn,
            typeKey,
            JsonConvert.SerializeObject(notification)));
    }
}
