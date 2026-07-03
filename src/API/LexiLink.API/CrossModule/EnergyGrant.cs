using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Domain;
using LexiLink.Modules.Energy.Application.Configuration.CrossModule;
using LexiLink.Modules.Energy.Application.Contracts;
using LexiLink.Modules.Energy.Application.PlayerEnergies.EnsurePlayerEnergyExists;
using LexiLink.Modules.Energy.Application.PlayerEnergies.GetPlayerEnergy;
using LexiLink.Modules.Energy.Application.PlayerEnergies.GrantEnergy;
using LexiLink.Modules.Quests.Application.Configuration.CrossModule;

namespace LexiLink.API.CrossModule;

// API-host adapter for Market -> Energy grants.
internal class EnergyGrant : IEnergyGrant, IQuestEnergyRewardGrant
{
    private readonly IEnergyModule _energyModule;

    public EnergyGrant(IEnergyModule energyModule)
    {
        _energyModule = energyModule;
    }

    public async Task EnsureCanAcceptAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            return;
        }

        await _energyModule.ExecuteCommandAsync(
            new EnsurePlayerEnergyExistsCommand(playerId),
            cancellationToken);

        var snapshot = await _energyModule.ExecuteQueryAsync(
            new GetPlayerEnergyQuery(playerId),
            cancellationToken);

        if (snapshot.CurrentAmount + amount > snapshot.MaximumAmount)
        {
            throw new BusinessRuleValidationException(
                new EnergyGrantMustFitWithinMaximumRule(
                    snapshot.CurrentAmount,
                    snapshot.MaximumAmount,
                    amount));
        }
    }

    public async Task<int> GrantEnergyRewardAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            return 0;
        }

        await _energyModule.ExecuteCommandAsync(
            new EnsurePlayerEnergyExistsCommand(playerId),
            cancellationToken);

        return await _energyModule.ExecuteCommandAsync(
            new GrantCappedEnergyCommand(playerId, amount),
            cancellationToken);
    }

    public Task GrantAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default)
    {
        return _energyModule.ExecuteCommandAsync(
            new GrantEnergyCommand(playerId, amount),
            cancellationToken);
    }

    private sealed class EnergyGrantMustFitWithinMaximumRule : IBusinessRule
    {
        private readonly int _currentAmount;
        private readonly int _maximumAmount;
        private readonly int _amount;

        public EnergyGrantMustFitWithinMaximumRule(
            int currentAmount,
            int maximumAmount,
            int amount)
        {
            _currentAmount = currentAmount;
            _maximumAmount = maximumAmount;
            _amount = amount;
        }

        public bool IsBroken() => _currentAmount + _amount > _maximumAmount;

        public string Message => "Energy reward cannot fit in the player's current energy capacity.";
    }
}
