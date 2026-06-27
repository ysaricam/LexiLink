using MediatR;
using LexiLink.Modules.Energy.Application.Configuration.Commands;
using LexiLink.Modules.Energy.Application.PlayerEnergies.GrantEnergy;

namespace LexiLink.Modules.Energy.Application.Admin.GrantBonusEnergy;

internal sealed class GrantBonusEnergyCommandHandler : ICommandHandler<GrantBonusEnergyCommand>
{
    private readonly ISender _sender;

    internal GrantBonusEnergyCommandHandler(ISender sender)
    {
        _sender = sender;
    }

    public Task Handle(GrantBonusEnergyCommand request, CancellationToken cancellationToken) =>
        // Wraps the internal GrantEnergyCommand so capped bonus behavior stays in one place.
        _sender.Send(new GrantEnergyCommand(request.PlayerId, request.Amount), cancellationToken);
}
