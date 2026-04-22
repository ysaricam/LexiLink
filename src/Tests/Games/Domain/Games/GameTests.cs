using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.GameLinks;
using LexiLink.Modules.Games.Domain.Score;
using LexiLink.Modules.Games.Domain.Games.Rules;
using LexiLink.Modules.Games.Domain.Categories;
using Xunit;

namespace LexiLink.Tests.Games.Domain.Games;

// Testler için basit bir resolver
public class StubResolver : ITargetLinkResolver
{
    public LinkId FixedTargetId { get; set; } = Link.CreateNew("Target").Id;
    public List<LinkId> FixedPathIds { get; set; } = new List<LinkId>();
    
    public TargetResolution Resolve(LinkId startLinkId, int depth) 
        => new TargetResolution(FixedTargetId, FixedPathIds);
}

public class GameTests
{
    private readonly StubResolver _stubResolver = new StubResolver();
    private readonly IScoreCalculator _scoreCalculator = new ScoreCalculator();
    private readonly GameCategoryId _categoryId = new GameCategoryId(Guid.NewGuid());

    [Fact]
    public void Create_ShouldInitializeWithNotStarted()
    {
        var startLink = Link.CreateNew("A");
        var game = Game.Create(_categoryId, startLink.Id, _stubResolver, _scoreCalculator);

        Assert.Equal(GameState.NotStarted, game.State);
        Assert.Equal(0, game.Score.Value);
        Assert.NotNull(game.Id);
    }

    [Fact]
    public void Create_ShouldThrow_WhenStartLinkIsNull()
    {
        var exception = Assert.Throws<BusinessRuleValidationException>(() => Game.Create(_categoryId, null!, _stubResolver, _scoreCalculator));
        Assert.IsType<GameStartLinkCannotBeNullRule>(exception.BrokenRule);
    }

    [Fact]
    public void Start_ShouldChangeStateToInProgress()
    {
        var startLink = Link.CreateNew("A");
        var game = Game.Create(_categoryId, startLink.Id, _stubResolver, _scoreCalculator);

        game.Start();

        Assert.Equal(GameState.InProgress, game.State);
    }

    [Fact]
    public void Start_ShouldThrow_WhenAlreadyInProgress()
    {
        var startLink = Link.CreateNew("A");
        var game = Game.Create(_categoryId, startLink.Id, _stubResolver, _scoreCalculator);
        game.Start();

        var exception = Assert.Throws<BusinessRuleValidationException>(() => game.Start());
        Assert.IsType<GameMustBeInSpecificStateRule>(exception.BrokenRule);
    }

    [Fact]
    public void MakeStep_ShouldCalculateScore_WhenTargetReached()
    {
        var target = Link.CreateNew("B");
        _stubResolver.FixedTargetId = target.Id;
        
        var root = Link.CreateNew("A", new[] { target.Id });
        var game = Game.Create(_categoryId, root.Id, _stubResolver, _scoreCalculator, targetDepth: 1, maxSteps: 5); 
        game.Start();

        game.MakeStep(target.Id, root.SubLinkIds);

        // targetDepth = 1 (100 base)
        // currentSteps = 1
        // maxSteps = 5
        // bonus = (5-1)*20 = 80
        // combo = 0 (Çünkü targetPath boş, kombo artmadı)
        // total = 100 + 80 + 0 = 180
        Assert.Equal(180, game.Score.Value);
        Assert.Equal(GameState.Completed, game.State);
    }

    [Fact]
    public void MakeStep_ShouldIncludeComboBonus_WhenCorrectPathFollowed()
    {
        var target = Link.CreateNew("C");
        var b = Link.CreateNew("B", new[] { target.Id });
        var root = Link.CreateNew("A", new[] { b.Id });

        _stubResolver.FixedTargetId = target.Id;
        _stubResolver.FixedPathIds = new List<LinkId> { b.Id, target.Id };

        var game = Game.Create(_categoryId, root.Id, _stubResolver, _scoreCalculator, targetDepth: 2, maxSteps: 10);
        game.Start();

        game.MakeStep(b.Id, root.SubLinkIds); // 1. kombo
        game.MakeStep(target.Id, b.SubLinkIds); // 2. kombo

        // targetDepth = 2 (200 base)
        // currentSteps = 2
        // maxSteps = 10
        // bonus = (10-2)*20 = 160
        // combo bonus = 2 * 50 = 100
        // total = 200 + 160 + 100 = 460
        Assert.Equal(460, game.Score.Value);
    }

    [Fact]
    public void MakeStep_ShouldThrow_WhenGameIsNotInProgress()
    {
        var startLink = Link.CreateNew("A");
        var game = Game.Create(_categoryId, startLink.Id, _stubResolver, _scoreCalculator);
        // Start() çağrılmadı, durum NotStarted
        
        var exception = Assert.Throws<BusinessRuleValidationException>(() => game.MakeStep(Link.CreateNew("B").Id, new List<LinkId>()));
        Assert.IsType<GameMustBeInSpecificStateRule>(exception.BrokenRule);
    }

    [Fact]
    public void MakeStep_ShouldKeepScoreAtZero_WhenFailed()
    {
        var target = Link.CreateNew("B");
        var root = Link.CreateNew("A", new[] { target.Id });
        
        _stubResolver.FixedTargetId = Link.CreateNew("C").Id;
        
        var game = Game.Create(_categoryId, root.Id, _stubResolver, _scoreCalculator, maxSteps: 1); 
        game.Start();
        
        game.MakeStep(target.Id, root.SubLinkIds); 
        
        Assert.Equal(GameState.Failed, game.State);
        Assert.Equal(0, game.Score.Value);
    }

    [Fact]
    public void MakeStep_ShouldThrowBusinessRuleException_WhenNotNextInChain()
    {
        var b = Link.CreateNew("B");
        var a = Link.CreateNew("A", new[] { b.Id });
        var x = Link.CreateNew("X");
        
        var game = Game.Create(_categoryId, a.Id, _stubResolver, _scoreCalculator);
        game.Start();

        var exception = Assert.Throws<BusinessRuleValidationException>(() => game.MakeStep(x.Id, a.SubLinkIds));
        Assert.IsType<NextStepMustBeSubLinkOfCurrentRule>(exception.BrokenRule);
    }

    [Fact]
    public void Fail_ShouldChangeStateToFailed()
    {
        var game = Game.Create(_categoryId, Link.CreateNew("A").Id, _stubResolver, _scoreCalculator);
        game.Start();
        
        game.Fail();
        
        Assert.Equal(GameState.Failed, game.State);
    }

    [Fact]
    public void UndoMove_ShouldRevertToPreviousLink()
    {
        var b = Link.CreateNew("B");
        var a = Link.CreateNew("A", new[] { b.Id });
        _stubResolver.FixedTargetId = Link.CreateNew("Target").Id;
        
        var game = Game.Create(_categoryId, a.Id, _stubResolver, _scoreCalculator);
        game.Start();

        game.MakeStep(b.Id, a.SubLinkIds);
        Assert.Equal(1, game.CurrentStep);

        game.UndoMove();
        Assert.Equal(0, game.CurrentStep);
        // A'ya geri döndüğünü doğrulamak için tekrar B'ye gitmeyi deneyebiliriz
        var result = game.MakeStep(b.Id, a.SubLinkIds); 
        Assert.Equal(MoveResult.Continue, result);
    }

    [Fact]
    public void UndoMove_ShouldThrow_WhenNoHistory()
    {
        var game = Game.Create(_categoryId, Link.CreateNew("A").Id, _stubResolver, _scoreCalculator);
        game.Start();

        var exception = Assert.Throws<BusinessRuleValidationException>(() => game.UndoMove());
        Assert.IsType<GameHasHistoryToUndoRule>(exception.BrokenRule);
    }

    [Fact]
    public void UndoMove_ShouldThrow_WhenLimitReached()
    {
        var b = Link.CreateNew("B");
        var a = Link.CreateNew("A", new[] { b.Id });
        _stubResolver.FixedTargetId = Link.CreateNew("Target").Id;
        var game = Game.Create(_categoryId, a.Id, _stubResolver, _scoreCalculator);
        game.Start();

        // 3 kez hamle yap ve geri al
        for(int i = 0; i < 3; i++)
        {
            game.MakeStep(b.Id, a.SubLinkIds);
            game.UndoMove();
        }

        // 4. hamle
        game.MakeStep(b.Id, a.SubLinkIds);
        var exception = Assert.Throws<BusinessRuleValidationException>(() => game.UndoMove());
        Assert.IsType<UndoLimitReachedRule>(exception.BrokenRule);
    }

    [Fact]
    public void GetHint_ShouldReturnNextStep_WhenOnCorrectPath()
    {
        var b = Link.CreateNew("B");
        var a = Link.CreateNew("A", new[] { b.Id });
        _stubResolver.FixedTargetId = b.Id;
        _stubResolver.FixedPathIds = new List<LinkId> { b.Id };

        var game = Game.Create(_categoryId, a.Id, _stubResolver, _scoreCalculator);
        game.Start();

        var hint = game.GetHint();

        Assert.Equal(HintStatus.OnCorrectPath, hint.Status);
        Assert.Equal(b.Id, hint.RecommendedLinkId);
    }

    [Fact]
    public void GetHint_ShouldRedirect_WhenOnWrongPath()
    {
        var b = Link.CreateNew("B");
        var x = Link.CreateNew("X");
        var a = Link.CreateNew("A", new[] { b.Id, x.Id });
        
        _stubResolver.FixedTargetId = b.Id;
        _stubResolver.FixedPathIds = new List<LinkId> { b.Id };

        var game = Game.Create(_categoryId, a.Id, _stubResolver, _scoreCalculator);
        game.Start();

        game.MakeStep(x.Id, a.SubLinkIds); // Yanlış yola saptık
        
        var hint = game.GetHint();

        Assert.Equal(HintStatus.RedirectedToSafety, hint.Status);
        Assert.Equal(b.Id, hint.RecommendedLinkId); // Doğru olan B'yi önermeli
        Assert.Equal(0, game.CurrentStep); // A'ya geri dönmüş olmalı
    }
}
