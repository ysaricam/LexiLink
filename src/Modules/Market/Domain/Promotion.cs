using LexiLink.Common.Domain;
using LexiLink.Modules.Market.Domain.Rules;

namespace LexiLink.Modules.Market.Domain;

public sealed class Promotion : ValueObject
{
    public int PromoPrice { get; }
    public DateTime StartsAt { get; }
    public DateTime EndsAt { get; }

    private Promotion()
    {
    }

    private Promotion(int promoPrice, DateTime startsAt, DateTime endsAt)
    {
        PromoPrice = promoPrice;
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public static Promotion Create(int promoPrice, int basePrice, DateTime startsAt, DateTime endsAt)
    {
        CheckRule(new WindowMustBeOrderedRule(startsAt, endsAt));
        CheckRule(new PromotionPriceMustBeLessThanPriceRule(promoPrice, basePrice));

        return new Promotion(promoPrice, startsAt, endsAt);
    }

    public bool IsOpenAt(DateTime now) => StartsAt <= now && now < EndsAt;

}
