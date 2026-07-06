using LexiLink.API.CrossModule;
using LexiLink.Modules.Undo.Application.Contracts;
using LexiLink.Modules.Undo.Application.PlayerUndoInventories.ConsumePlayerUndo;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;
using NSubstitute;

namespace LexiLink.API.Tests.CrossModule;

[TestFixture]
public sealed class UndoGuardTests
{
    [Test]
    public async Task EnsureUndoAvailableAsync_WhenUnlimitedGameplayUndoIsEnabled_DoesNotConsumeInventory()
    {
        var undoModule = Substitute.For<IUndoModule>();
        var sut = new UndoGuard(
            undoModule,
            new TestUndoConfiguration(unlimitedGameplayUndoEnabled: true));

        await sut.EnsureUndoAvailableAsync(Guid.NewGuid());

        await undoModule.DidNotReceive()
            .ExecuteCommandAsync(Arg.Any<ICommand>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task EnsureUndoAvailableAsync_WhenUnlimitedGameplayUndoIsDisabled_ConsumesOneUndo()
    {
        var playerId = Guid.NewGuid();
        var undoModule = Substitute.For<IUndoModule>();
        var sut = new UndoGuard(
            undoModule,
            new TestUndoConfiguration(unlimitedGameplayUndoEnabled: false));

        await sut.EnsureUndoAvailableAsync(playerId);

        await undoModule.Received(1).ExecuteCommandAsync(
            Arg.Is<ConsumePlayerUndoCommand>(command =>
                command.PlayerId == playerId &&
                command.Amount == 1),
            Arg.Any<CancellationToken>());
    }

    private sealed class TestUndoConfiguration : IUndoConfigurationService
    {
        public TestUndoConfiguration(bool unlimitedGameplayUndoEnabled)
        {
            UnlimitedGameplayUndoEnabled = unlimitedGameplayUndoEnabled;
        }

        public int InitialBalance => 0;
        public bool UnlimitedGameplayUndoEnabled { get; }
        public int UnlimitedGameplayBalance => 999_999;
    }
}
