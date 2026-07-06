using LexiLink.Modules.Undo.Application.PlayerUndoInventories.GetPlayerUndo;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;

namespace LexiLink.Modules.Undo.Tests.PlayerUndoInventories;

[TestFixture]
public sealed class PlayerUndoSnapshotPresenterTests
{
    private static readonly Guid PlayerId = new("11111111-1111-1111-1111-111111111111");

    [Test]
    public void ApplyGameplayPresentation_WhenUnlimitedGameplayUndoIsDisabled_ReturnsRealBalance()
    {
        var snapshot = new PlayerUndoSnapshotDto(PlayerId, Balance: 0);

        var result = PlayerUndoSnapshotPresenter.ApplyGameplayPresentation(
            snapshot,
            new TestUndoConfiguration(
                unlimitedGameplayUndoEnabled: false,
                unlimitedGameplayBalance: 999_999),
            useGameplayPresentation: true);

        result.Balance.Should().Be(0);
    }

    [Test]
    public void ApplyGameplayPresentation_WhenUnlimitedGameplayUndoIsEnabled_ReturnsConfiguredGameplayBalance()
    {
        var snapshot = new PlayerUndoSnapshotDto(PlayerId, Balance: 0);

        var result = PlayerUndoSnapshotPresenter.ApplyGameplayPresentation(
            snapshot,
            new TestUndoConfiguration(
                unlimitedGameplayUndoEnabled: true,
                unlimitedGameplayBalance: 123_456),
            useGameplayPresentation: true);

        result.Balance.Should().Be(123_456);
        result.PlayerId.Should().Be(PlayerId);
    }

    [Test]
    public void ApplyGameplayPresentation_WhenGameplayPresentationIsDisabled_ReturnsRealBalanceForAdminViews()
    {
        var snapshot = new PlayerUndoSnapshotDto(PlayerId, Balance: 7);

        var result = PlayerUndoSnapshotPresenter.ApplyGameplayPresentation(
            snapshot,
            new TestUndoConfiguration(
                unlimitedGameplayUndoEnabled: true,
                unlimitedGameplayBalance: 999_999),
            useGameplayPresentation: false);

        result.Balance.Should().Be(7);
    }

    [Test]
    public void PresentOrCreateForGameplay_WhenInventoryIsMissingAndUnlimitedGameplayUndoIsEnabled_ReturnsSyntheticBalance()
    {
        var result = PlayerUndoSnapshotPresenter.PresentOrCreateForGameplay(
            snapshot: null,
            playerId: PlayerId,
            new TestUndoConfiguration(
                unlimitedGameplayUndoEnabled: true,
                unlimitedGameplayBalance: 999_999),
            useGameplayPresentation: true);

        result.Should().NotBeNull();
        result!.PlayerId.Should().Be(PlayerId);
        result.Balance.Should().Be(999_999);
    }

    [Test]
    public void PresentOrCreateForGameplay_WhenInventoryIsMissingForAdminView_ReturnsNull()
    {
        var result = PlayerUndoSnapshotPresenter.PresentOrCreateForGameplay(
            snapshot: null,
            playerId: PlayerId,
            new TestUndoConfiguration(
                unlimitedGameplayUndoEnabled: true,
                unlimitedGameplayBalance: 999_999),
            useGameplayPresentation: false);

        result.Should().BeNull();
    }

    private sealed class TestUndoConfiguration : IUndoConfigurationService
    {
        public TestUndoConfiguration(
            bool unlimitedGameplayUndoEnabled,
            int unlimitedGameplayBalance)
        {
            UnlimitedGameplayUndoEnabled = unlimitedGameplayUndoEnabled;
            UnlimitedGameplayBalance = unlimitedGameplayBalance;
        }

        public int InitialBalance => 0;
        public bool UnlimitedGameplayUndoEnabled { get; }
        public int UnlimitedGameplayBalance { get; }
    }
}
