using LexiLink.Common.Domain;

namespace LexiLink.Modules.Hint.Domain.PlayerHintInventories.Events;

public class PlayerHintConsumedDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int Amount { get; }
    public int RemainingBalance { get; }
    public DateTime ConsumedOn { get; }

    public PlayerHintConsumedDomainEvent(Guid playerId, int amount, int remainingBalance, DateTime consumedOn)
    {
        PlayerId = playerId;
        Amount = amount;
        RemainingBalance = remainingBalance;
        ConsumedOn = consumedOn;
    }
}
