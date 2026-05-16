using LexiLink.Modules.Energy.Application.Contracts;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.GetPlayerEnergy;

public class GetPlayerEnergyQuery : QueryBase<PlayerEnergySnapshotDto>
{
    public Guid PlayerId { get; }

    public GetPlayerEnergyQuery(Guid playerId)
    {
        PlayerId = playerId;
    }
}
