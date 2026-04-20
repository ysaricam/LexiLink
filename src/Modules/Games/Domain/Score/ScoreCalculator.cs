namespace LexiLink.Modules.Games.Domain.Score;

public sealed class ScoreCalculator : IScoreCalculator
{
    private const int BasePointPerDepth = 100;
    private const int BonusPerRemainingStep = 20;
    private const int ComboBonusFactor = 50;

    public ScoreValue Calculate(int targetDepth, int currentSteps, int maxSteps, int comboCount)
    {
        if (currentSteps <= 0) return ScoreValue.Zero;

        // Hedef derinlik arttıkça taban puan artar (ör: 3 derinlik = 300 puan)
        int basePoints = targetDepth * BasePointPerDepth;

        // Kalan hamle başına bonus eklenir (ör: 10 max - 4 adım = 6 * 20 = 120 bonus)
        int remainingSteps = Math.Max(0, maxSteps - currentSteps);
        int bonusPoints = remainingSteps * BonusPerRemainingStep;

        // Eğer oyuncu hedef derinlikten daha kısa sürede bulduysa ekstra çarpan (Efficiency Bonus)
        if (currentSteps < targetDepth)
        {
            bonusPoints += (targetDepth - currentSteps) * 50;
        }

        // Kombo Bonusu: Peş peşe doğru hamlelerin toplamı
        int comboBonus = comboCount * ComboBonusFactor;

        return ScoreValue.Of(basePoints + bonusPoints + comboBonus);
    }
}
