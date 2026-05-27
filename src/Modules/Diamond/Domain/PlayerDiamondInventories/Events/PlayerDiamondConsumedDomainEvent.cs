using LexiLink.Common.Domain;

namespace LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories.Events;

public class PlayerDiamondConsumedDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int Amount { get; }
    public int RemainingBalance { get; }
    public DateTime ConsumedOn { get; }

    public PlayerDiamondConsumedDomainEvent(Guid playerId, int amount, int remainingBalance, DateTime consumedOn)
    {
        PlayerId = playerId;
        Amount = amount;
        RemainingBalance = remainingBalance;
        ConsumedOn = consumedOn;
    }
}
