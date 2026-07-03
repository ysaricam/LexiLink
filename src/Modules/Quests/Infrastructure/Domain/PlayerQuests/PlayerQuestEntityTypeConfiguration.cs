using LexiLink.Modules.Quests.Domain.PlayerQuests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Quests.Infrastructure.Domain.PlayerQuests;

internal class PlayerQuestEntityTypeConfiguration : IEntityTypeConfiguration<PlayerQuest>
{
    public void Configure(EntityTypeBuilder<PlayerQuest> builder)
    {
        builder.ToTable("PlayerQuests", "quests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");

        builder.Property<Guid>("_playerId").HasColumnName("PlayerId");
        builder.Property<QuestDefinitionId>("_questDefinitionId").HasColumnName("QuestDefinitionId");
        builder.Property<int>("_progressBaselineSnapshot").HasColumnName("ProgressBaselineSnapshot");
        builder.Property<int>("_remainingEnergyReward").HasColumnName("RemainingEnergyReward");
        builder.Property<bool>("_nonEnergyRewardsClaimed").HasColumnName("NonEnergyRewardsClaimed");
        builder.Property<QuestState>("_state")
            .HasColumnName("State")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property<DateTime>("_issuedAt").HasColumnName("IssuedAt");
        builder.Property<DateTime?>("_claimedAt").HasColumnName("ClaimedAt");
        builder.Property<DateTime?>("_expiresAt").HasColumnName("ExpiresAt");
    }
}
