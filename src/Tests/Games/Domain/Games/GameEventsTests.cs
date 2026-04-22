using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.GameLinks;
using LexiLink.Modules.Games.Domain.Score;
using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Games.Events;
using Xunit;

namespace LexiLink.Tests.Games.Domain.Games;

public class GameEventsTests
{
    private readonly StubResolver _stubResolver = new StubResolver();
    private readonly IScoreCalculator _scoreCalculator = new ScoreCalculator();
    private readonly GameCategoryId _categoryId = new GameCategoryId(Guid.NewGuid());

    private T? GetPublishedEvent<T>(Game game) where T : class, IDomainEvent
    {
        return game.DomainEvents?.OfType<T>().SingleOrDefault();
    }

    [Fact]
    public void Create_ShouldPublishGameCreatedDomainEvent()
    {
        var startLink = Link.CreateNew("A");
        var game = Game.Create(_categoryId, startLink.Id, _stubResolver, _scoreCalculator);

        var domainEvent = GetPublishedEvent<GameCreatedDomainEvent>(game);

        Assert.NotNull(domainEvent);
        Assert.Equal(game.Id, domainEvent!.GameId);
        Assert.Equal(_categoryId, domainEvent.CategoryId);
    }

    [Fact]
    public void Start_ShouldPublishGameStartedDomainEvent()
    {
        var startLink = Link.CreateNew("A");
        var targetLink = Link.CreateNew("Target");
        _stubResolver.FixedTargetId = targetLink.Id;
        
        var game = Game.Create(_categoryId, startLink.Id, _stubResolver, _scoreCalculator, targetDepth: 3, maxSteps: 10);
        game.Start();

        var domainEvent = GetPublishedEvent<GameStartedDomainEvent>(game);

        Assert.NotNull(domainEvent);
        Assert.Equal(game.Id, domainEvent!.GameId);
        Assert.Equal(startLink.Id, domainEvent.StartLinkId);
        Assert.Equal(targetLink.Id, domainEvent.TargetLinkId);
        Assert.Equal(3, domainEvent.TargetDepth);
        Assert.Equal(10, domainEvent.MaxSteps);
    }

    [Fact]
    public void MakeStep_ShouldPublishGameStepMadeDomainEvent()
    {
        var b = Link.CreateNew("B");
        var a = Link.CreateNew("A", new[] { b.Id });
        _stubResolver.FixedPathIds = new List<LinkId> { b.Id };
        
        var game = Game.Create(_categoryId, a.Id, _stubResolver, _scoreCalculator);
        game.Start();
        game.ClearDomainEvents(); // Start event'ini temizleyelim

        game.MakeStep(b.Id, a.SubLinkIds);

        var domainEvent = GetPublishedEvent<GameStepMadeDomainEvent>(game);

        Assert.NotNull(domainEvent);
        Assert.Equal(a.Id, domainEvent!.CurrentLinkId);
        Assert.Equal(b.Id, domainEvent.NextLinkId);
        Assert.Equal(1, domainEvent.StepNumber);
        Assert.True(domainEvent.IsCorrectStep);
    }

    [Fact]
    public void MakeStep_ShouldPublishGameCompletedDomainEvent_WhenTargetReached()
    {
        var target = Link.CreateNew("B");
        var root = Link.CreateNew("A", new[] { target.Id });
        _stubResolver.FixedTargetId = target.Id;
        
        var game = Game.Create(_categoryId, root.Id, _stubResolver, _scoreCalculator);
        game.Start();
        game.ClearDomainEvents();

        game.MakeStep(target.Id, root.SubLinkIds);

        var domainEvent = GetPublishedEvent<GameCompletedDomainEvent>(game);

        Assert.NotNull(domainEvent);
        Assert.Equal(game.Score, domainEvent!.FinalScore);
        Assert.Equal(1, domainEvent.TotalSteps);
    }

    [Fact]
    public void GetHint_ShouldPublishGameHintUsedDomainEvent()
    {
        var b = Link.CreateNew("B");
        var a = Link.CreateNew("A", new[] { b.Id });
        _stubResolver.FixedTargetId = b.Id;
        _stubResolver.FixedPathIds = new List<LinkId> { b.Id };

        var game = Game.Create(_categoryId, a.Id, _stubResolver, _scoreCalculator);
        game.Start();
        game.ClearDomainEvents();

        game.GetHint();

        var domainEvent = GetPublishedEvent<GameHintUsedDomainEvent>(game);

        Assert.NotNull(domainEvent);
        Assert.Equal(HintStatus.OnCorrectPath, domainEvent!.HintStatus);
        Assert.Equal(0, domainEvent.StepNumber);
        Assert.Equal(a.Id, domainEvent.CurrentLinkId);
    }

    [Fact]
    public void GetHint_ShouldPublishGameHintUsedDomainEvent_WithCorrectData_WhenOnWrongPath()
    {
        var target = Link.CreateNew("Target");
        var x = Link.CreateNew("X");
        var a = Link.CreateNew("A", new[] { target.Id, x.Id });
        _stubResolver.FixedTargetId = target.Id;
        _stubResolver.FixedPathIds = new List<LinkId> { target.Id };

        var game = Game.Create(_categoryId, a.Id, _stubResolver, _scoreCalculator);
        game.Start();
        game.MakeStep(x.Id, a.SubLinkIds); // Wrong path
        game.ClearDomainEvents();

        game.GetHint();

        var domainEvent = GetPublishedEvent<GameHintUsedDomainEvent>(game);

        Assert.NotNull(domainEvent);
        Assert.Equal(HintStatus.RedirectedToSafety, domainEvent!.HintStatus);
        Assert.Equal(0, domainEvent.StepNumber); // Reset to 0 by hint
        Assert.Equal(a.Id, domainEvent.CurrentLinkId); // Back at A
    }

    [Fact]
    public void UndoMove_ShouldPublishGameMoveUndoneDomainEvent()
    {
        var b = Link.CreateNew("B");
        var a = Link.CreateNew("A", new[] { b.Id });
        var game = Game.Create(_categoryId, a.Id, _stubResolver, _scoreCalculator);
        game.Start();
        game.MakeStep(b.Id, a.SubLinkIds);
        game.ClearDomainEvents();

        game.UndoMove();

        var domainEvent = GetPublishedEvent<GameMoveUndoneDomainEvent>(game);

        Assert.NotNull(domainEvent);
        Assert.Equal(a.Id, domainEvent!.CurrentLinkId);
        Assert.Equal(b.Id, domainEvent.PreviousLinkId);
    }

    [Fact]
    public void ResetToStart_ShouldPublishGameResetToStartDomainEvent()
    {
        var b = Link.CreateNew("B");
        var a = Link.CreateNew("A", new[] { b.Id });
        var game = Game.Create(_categoryId, a.Id, _stubResolver, _scoreCalculator);
        game.Start();
        game.MakeStep(b.Id, a.SubLinkIds);
        game.ClearDomainEvents();

        game.ResetToStart();

        var domainEvent = GetPublishedEvent<GameResetToStartDomainEvent>(game);

        Assert.NotNull(domainEvent);
        Assert.Equal(b.Id, domainEvent!.ResetFromLinkId);
        Assert.Equal(1, domainEvent.StepNumberAtReset);
    }

    [Fact]
    public void Fail_ShouldPublishGameFailedDomainEventWithManualReason()
    {
        var game = Game.Create(_categoryId, Link.CreateNew("A").Id, _stubResolver, _scoreCalculator);
        game.Start();
        game.ClearDomainEvents();

        game.Fail();

        var domainEvent = GetPublishedEvent<GameFailedDomainEvent>(game);

        Assert.NotNull(domainEvent);
        Assert.Equal("ManualFail", domainEvent!.Reason);
    }
}