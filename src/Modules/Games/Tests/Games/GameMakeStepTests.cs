using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Games.Events;
using LexiLink.Modules.Games.Domain.Games.Rules;
using LexiLink.Modules.Games.Domain.Links;
using LexiLink.Modules.Games.Tests.SeedWork;

namespace LexiLink.Modules.Games.Tests.Games;

[TestFixture]
public class GameMakeStepTests : GameTestsBase
{
    [Test]
    public void MakeStep_FromInProgressToValidNeighbor_AdvancesAndRaisesStepMadeDomainEvent()
    {
        var start = NewLinkId();
        var mid = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [mid, target]);
        var game = BuildGame(puzzle, maxSteps: 5);
        game.Start();
        DomainEventsTestHelper.ClearAllDomainEvents(game);

        game.MakeStep(mid, LinearNeighborResolver(start, mid, target), FixedScoreCalculator());

        game.CurrentLinkId.Should().Be(mid);
        game.History.Should().ContainSingle(id => id == mid);
        AssertPublishedDomainEvent<StepMadeDomainEvent>(game)
            .LinkId.Should().Be(mid);
    }

    [Test]
    public void MakeStep_WhenNotStarted_BreaksGameMustBeInProgressRule()
    {
        var start = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [target]);
        var game = BuildGame(puzzle);

        AssertBrokenRule<GameMustBeInProgressRule>(
            () => game.MakeStep(target, LinearNeighborResolver(start, target), FixedScoreCalculator()));
    }

    [Test]
    public void MakeStep_WhenAbandoned_BreaksGameMustBeInProgressRule()
    {
        var start = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [target]);
        var game = BuildGame(puzzle);
        game.Start();
        game.Abandon();

        AssertBrokenRule<GameMustBeInProgressRule>(
            () => game.MakeStep(target, LinearNeighborResolver(start, target), FixedScoreCalculator()));
    }

    [Test]
    public void MakeStep_ToInvalidNeighbor_BreaksStepMustBeValidRule()
    {
        var start = NewLinkId();
        var validNext = NewLinkId();
        var invalidNext = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [validNext, target]);
        var game = BuildGame(puzzle, maxSteps: 5);
        game.Start();

        AssertBrokenRule<StepMustBeValidRule>(
            () => game.MakeStep(invalidNext, LinearNeighborResolver(start, validNext, target), FixedScoreCalculator()));
    }

    [Test]
    public void MakeStep_OntoTargetLink_CompletesAndRaisesGameCompletedDomainEvent()
    {
        var start = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [target]);
        var game = BuildGame(puzzle, maxSteps: 5);
        game.Start();
        DomainEventsTestHelper.ClearAllDomainEvents(game);

        game.MakeStep(target, LinearNeighborResolver(start, target), FixedScoreCalculator(points: 250));

        game.State.Should().Be(GameState.Completed);
        game.Score.Should().NotBeNull();
        game.Score!.Points.Should().Be(250);
        var domainEvent = AssertPublishedDomainEvent<GameCompletedDomainEvent>(game);
        domainEvent.GameId.Should().Be(game.Id);
        domainEvent.PlayerId.Should().Be(game.PlayerId);
        domainEvent.StartLinkId.Should().Be(start);
        domainEvent.TargetLinkId.Should().Be(target);
    }

    [Test]
    public void MakeStep_AtPenultimateBudgetSlot_RaisesLastStepWarningIssuedDomainEvent()
    {
        var start = NewLinkId();
        var n1 = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [n1, target]);
        // maxSteps=3 → after 2nd step Taken=2 == Max-1 → IsAtLastWarning
        var game = BuildGame(puzzle, maxSteps: 3);
        game.Start();
        var resolver = LinearNeighborResolver(start, n1, target);

        game.MakeStep(n1, resolver, FixedScoreCalculator()); // Taken=1, no warning
        DomainEventsTestHelper.ClearAllDomainEvents(game);

        // For the warning we need a non-target step on the last slot. Use a self-loop neighbor.
        var sideStep = NewLinkId();
        var resolver2 = NeighborResolver((n1, new List<LinkId> { sideStep }), (sideStep, new List<LinkId>()));
        game.MakeStep(sideStep, resolver2, FixedScoreCalculator()); // Taken=2 == Max-1 → warning

        game.State.Should().Be(GameState.LastStepWarning);
        AssertPublishedDomainEvent<LastStepWarningIssuedDomainEvent>(game)
            .GameId.Should().Be(game.Id);
    }

    [Test]
    public void MakeStep_WhenBudgetExhaustedWithoutHittingTarget_FailsAndRaisesGameFailedDomainEvent()
    {
        var start = NewLinkId();
        var n1 = NewLinkId();
        var n2 = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [target]);
        // maxSteps=2 — two steps that don't reach target should fail.
        var game = BuildGame(puzzle, maxSteps: 2);
        game.Start();
        game.MakeStep(n1, NeighborResolver((start, [n1])), FixedScoreCalculator());
        DomainEventsTestHelper.ClearAllDomainEvents(game);

        game.MakeStep(n2, NeighborResolver((n1, [n2])), FixedScoreCalculator());

        game.State.Should().Be(GameState.Failed);
        AssertPublishedDomainEvent<GameFailedDomainEvent>(game)
            .GameId.Should().Be(game.Id);
    }
}
