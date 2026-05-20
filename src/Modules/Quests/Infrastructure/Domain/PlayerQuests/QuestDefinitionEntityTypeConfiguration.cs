using LexiLink.Modules.Quests.Domain.PlayerQuests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Quests.Infrastructure.Domain.PlayerQuests;

internal sealed class QuestDefinitionEntityTypeConfiguration : IEntityTypeConfiguration<QuestDefinition>
{
    public void Configure(EntityTypeBuilder<QuestDefinition> builder)
    {
        builder.ToTable("QuestDefinitions", "quests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");

        builder.Property<QuestType>("_questType")
            .HasColumnName("QuestType")
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property<QuestCadence>("_cadence")
            .HasColumnName("Cadence")
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property<int>("_goal").HasColumnName("Goal");
        builder.Property<int>("_rewardAmount").HasColumnName("RewardAmount");

        builder.Property<QuestType?>("_prerequisiteQuestType")
            .HasColumnName("PrerequisiteQuestType")
            .HasConversion<string?>()
            .HasMaxLength(64);

        builder.Property<bool>("_isActive").HasColumnName("IsActive");
    }
}
