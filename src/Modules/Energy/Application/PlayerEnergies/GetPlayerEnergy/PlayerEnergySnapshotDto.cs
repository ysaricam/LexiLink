namespace LexiLink.Modules.Energy.Application.PlayerEnergies.GetPlayerEnergy;

public record PlayerEnergySnapshotDto(
    Guid PlayerId,
    int CurrentAmount,
    int MaximumAmount,
    bool IsFull,
    int RechargeIntervalSeconds,
    DateTime LastRefilledOn,
    int? SecondsUntilNextRefill,
    DateTime? FullyRefilledAt);
