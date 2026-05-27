using LexiLink.Modules.Diamond.Application.Configuration.Commands;
using LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.GrantDiamond;
using MediatR;

namespace LexiLink.Modules.Diamond.Application.Admin.GrantBonusDiamond;

internal sealed class GrantBonusDiamondCommandHandler : ICommandHandler<GrantBonusDiamondCommand>
{
    private readonly ISender _sender;

    internal GrantBonusDiamondCommandHandler(ISender sender)
    {
        _sender = sender;
    }

    public Task Handle(GrantBonusDiamondCommand request, CancellationToken cancellationToken) =>
        // Keep the over-balance bonus behavior in one internal command.
        _sender.Send(new GrantDiamondCommand(request.PlayerId, request.Amount), cancellationToken);
}
