namespace LexiLink.Modules.Energy.Domain.PlayerEnergies;

internal readonly record struct EnergyRefillProjection(int CurrentAmount, DateTime LastRefilledOn);

internal static class EnergyRefillCalculator
{
    public static EnergyRefillProjection Project(
        int currentAmount,
        int maximumAmount,
        DateTime lastRefilledOn,
        int rechargeIntervalSeconds,
        DateTime now)
    {
        if (currentAmount >= maximumAmount)
        {
            return new EnergyRefillProjection(currentAmount, lastRefilledOn);
        }

        var elapsed = now - lastRefilledOn;
        if (elapsed <= TimeSpan.Zero)
        {
            return new EnergyRefillProjection(currentAmount, lastRefilledOn);
        }

        var interval = TimeSpan.FromSeconds(rechargeIntervalSeconds);
        var possibleTicks = (int)(elapsed.Ticks / interval.Ticks);
        if (possibleTicks <= 0)
        {
            return new EnergyRefillProjection(currentAmount, lastRefilledOn);
        }

        var slotsRemaining = maximumAmount - currentAmount;
        var actualTicks = Math.Min(possibleTicks, slotsRemaining);

        return new EnergyRefillProjection(
            currentAmount + actualTicks,
            lastRefilledOn.AddSeconds((double)actualTicks * rechargeIntervalSeconds));
    }
}
