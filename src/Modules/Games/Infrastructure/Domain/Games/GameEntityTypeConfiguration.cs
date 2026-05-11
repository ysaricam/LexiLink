using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Games.Allowances;
using LexiLink.Modules.Games.Domain.Games.Puzzles;
using LexiLink.Modules.Games.Domain.Links;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Games.Infrastructure.Domain.Games;

internal class GameEntityTypeConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games", "games");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlayerId).HasColumnName("PlayerId");
        builder.Property<LinkId>("_currentLinkId").HasColumnName("CurrentLinkId");
        builder.Property<GameState>("_gameState")
            .HasColumnName("State")
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.OwnsOne<Puzzle>("_puzzle", p =>
        {
            p.Property(x => x.CategoryId).HasColumnName("CategoryId");
            p.Property(x => x.Difficulty)
                .HasColumnName("Difficulty")
                .HasConversion<string>()
                .HasMaxLength(32);
            p.Property(x => x.StartLinkId).HasColumnName("StartLinkId");
            p.Property(x => x.TargetLinkId).HasColumnName("TargetLinkId");

            p.OwnsMany<OptimalPathStep>("_optimalPath", op =>
            {
                op.ToTable("GameOptimalPath", "games");
                op.WithOwner().HasForeignKey("GameId");
                op.Property(x => x.Position).HasColumnName("Position").ValueGeneratedNever();
                op.Property(x => x.LinkId).HasColumnName("LinkId");
                op.HasKey("GameId", "Position");
            });
            p.Navigation("_optimalPath").UsePropertyAccessMode(PropertyAccessMode.Field);
        });
        builder.Navigation("_puzzle").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsOne<Score>("_score", s =>
        {
            s.Property(x => x.Points).HasColumnName("Score");
        });
        builder.Navigation("_score")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .IsRequired(false);

        builder.OwnsOne<StepBudget>("_stepBudget", b =>
        {
            b.Property(x => x.Max).HasColumnName("MaxSteps");
            b.Property(x => x.Taken).HasColumnName("StepsTaken");
        });
        builder.Navigation("_stepBudget").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsOne<HintAllowance>("_hintAllowance", h =>
        {
            h.Property(x => x.Remaining).HasColumnName("HintsRemaining");
            h.Property(x => x.Used).HasColumnName("HintsUsed");
        });
        builder.Navigation("_hintAllowance").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsOne<UndoAllowance>("_undoAllowance", u =>
        {
            u.Property(x => x.Remaining).HasColumnName("UndosRemaining");
            u.Property(x => x.Used).HasColumnName("UndosUsed");
        });
        builder.Navigation("_undoAllowance").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsOne<ResetAllowance>("_resetAllowance", r =>
        {
            r.Property(x => x.Remaining).HasColumnName("ResetsRemaining");
            r.Property(x => x.Used).HasColumnName("ResetsUsed");
        });
        builder.Navigation("_resetAllowance").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany<GameHistoryStep>("_history", h =>
        {
            h.ToTable("GameHistory", "games");
            h.WithOwner().HasForeignKey("GameId");
            h.Property(x => x.StepNumber).HasColumnName("StepNumber").ValueGeneratedNever();
            h.Property(x => x.LinkId).HasColumnName("LinkId");
            h.HasKey("GameId", "StepNumber");
        });
        builder.Navigation("_history").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
