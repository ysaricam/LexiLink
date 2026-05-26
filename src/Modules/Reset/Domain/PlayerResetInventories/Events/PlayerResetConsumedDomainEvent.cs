using LexiLink.Common.Domain;

namespace LexiLink.Modules.Reset.Domain.PlayerResetInventories.Events;

public class PlayerResetConsumedDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int Amount { get; }
    public int RemainingBalance { get; }
    public DateTime ConsumedOn { get; }

    public PlayerResetConsumedDomainEvent(Guid playerId, int amount, int remainingBalance, DateTime consumedOn)
    {
        PlayerId = playerId;
        Amount = amount;
        RemainingBalance = remainingBalance;
        ConsumedOn = consumedOn;
    }
}
