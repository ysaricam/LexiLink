using MediatR;
using LexiLink.Modules.Hint.Application.Configuration.Commands;
using LexiLink.Modules.Hint.Application.PlayerHintInventories.GrantHint;

namespace LexiLink.Modules.Hint.Application.Admin.GrantBonusHint;

internal sealed class GrantBonusHintCommandHandler : ICommandHandler<GrantBonusHintCommand>
{
    private readonly ISender _sender;

    internal GrantBonusHintCommandHandler(ISender sender)
    {
        _sender = sender;
    }

    public Task Handle(GrantBonusHintCommand request, CancellationToken cancellationToken) =>
        // Wraps the internal GrantHintCommand so the bonus path
        // (over-cap allowed — hint inventory has no cap) stays in
        // one place.
        _sender.Send(new GrantHintCommand(request.PlayerId, request.Amount), cancellationToken);
}
