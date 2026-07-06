using LexiLink.Modules.Undo.Infrastructure.Domain.PlayerUndoInventories;
using Microsoft.Extensions.Configuration;

namespace LexiLink.Modules.Undo.Tests.PlayerUndoInventories;

[TestFixture]
public sealed class UndoConfigurationServiceTests
{
    [Test]
    public void Constructor_WhenConfigIsEmpty_UsesSafeDefaults()
    {
        var sut = new UndoConfigurationService(BuildConfiguration());

        sut.InitialBalance.Should().Be(0);
        sut.UnlimitedGameplayUndoEnabled.Should().BeFalse();
        sut.UnlimitedGameplayBalance.Should().Be(999_999);
    }

    [Test]
    public void Constructor_WhenUnlimitedGameplayUndoIsConfigured_ReadsValues()
    {
        var sut = new UndoConfigurationService(BuildConfiguration(
            new KeyValuePair<string, string?>("Undo:InitialBalance", "2"),
            new KeyValuePair<string, string?>("Undo:UnlimitedGameplayUndo", "true"),
            new KeyValuePair<string, string?>("Undo:UnlimitedGameplayBalance", "123456")));

        sut.InitialBalance.Should().Be(2);
        sut.UnlimitedGameplayUndoEnabled.Should().BeTrue();
        sut.UnlimitedGameplayBalance.Should().Be(123_456);
    }

    [Test]
    public void Constructor_WhenUnlimitedGameplayBalanceIsNotPositive_FallsBackToPositiveDefault()
    {
        var sut = new UndoConfigurationService(BuildConfiguration(
            new KeyValuePair<string, string?>("Undo:UnlimitedGameplayUndo", "true"),
            new KeyValuePair<string, string?>("Undo:UnlimitedGameplayBalance", "0")));

        sut.UnlimitedGameplayUndoEnabled.Should().BeTrue();
        sut.UnlimitedGameplayBalance.Should().Be(999_999);
    }

    [Test]
    public void Constructor_WhenUnlimitedGameplayUndoFlagIsInvalid_KeepsFeatureDisabled()
    {
        var sut = new UndoConfigurationService(BuildConfiguration(
            new KeyValuePair<string, string?>("Undo:UnlimitedGameplayUndo", "not-a-bool")));

        sut.UnlimitedGameplayUndoEnabled.Should().BeFalse();
    }

    private static IConfiguration BuildConfiguration(params KeyValuePair<string, string?>[] values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
