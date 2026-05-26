using MediatR;
using LexiLink.Modules.Reset.Application.Configuration.Commands;
using LexiLink.Modules.Reset.Application.PlayerResetInventories.GrantReset;

namespace LexiLink.Modules.Reset.Application.Admin.GrantBonusReset;

internal sealed class GrantBonusResetCommandHandler : ICommandHandler<GrantBonusResetCommand>
{
    private readonly ISender _sender;

    internal GrantBonusResetCommandHandler(ISender sender)
    {
        _sender = sender;
    }

    public Task Handle(GrantBonusResetCommand request, CancellationToken cancellationToken) =>
        // Wraps the internal GrantResetCommand so the bonus path
        // (over-cap allowed — reset inventory has no cap) stays in
        // one place.
        _sender.Send(new GrantResetCommand(request.PlayerId, request.Amount), cancellationToken);
}
