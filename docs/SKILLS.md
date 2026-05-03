# SKILLS.md

Project rules for LexiLink — a Modular Monolith built on Domain-Driven Design.

These rules are derived from **Kamil Grzybek's** [modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd) reference, plus a few project-specific decisions explicitly recorded in `activeContext.md`. Every rule below is currently enforced in the codebase. When a future change tempts you to violate one, push back or flag the tradeoff before writing code.

Format: rule heading, 1–2 paragraph rationale, **DO** / **DON'T** snippet.

---

## 1. Aggregate Construction

Every aggregate root has three constructors: a `private` parameterless ctor for EF Core, a `private` primary ctor that holds the invariants, and an `internal static Create(...)` factory. The factory is the only way to bring an aggregate into existence. **Never** add a `public` constructor — it bypasses invariants and removes the single chokepoint where rules are enforced.

This pattern lives in `src/Modules/Games/Domain/Categories/Category.cs`, `Links/Link.cs`, and `Games/Game.cs`. The `internal` modifier on `Create` keeps factory access scoped to the same Domain assembly so Application code uses repositories or domain services to obtain aggregates rather than constructing them directly.

**DO**
```csharp
public class Category : Entity, IAggregateRoot
{
    private Category() { }                                    // EF Core
    private Category(CategoryId id, string name, string? desc) // invariants here
    {
        CheckRule(new CategoryNameMustNotBeEmptyRule(name));
        CheckRule(new CategoryNameMustNotExceedMaxLengthRule(name));
        Id = id; _name = name; _description = desc;
    }
    internal static Category Create(string name, string? description)
        => new(new CategoryId(Guid.NewGuid()), name, description);
}
```

**DON'T**
```csharp
public class Category : Entity, IAggregateRoot
{
    public Category(string name) { _name = name; }   // public ctor bypasses rules
    public Category() { }                            // anyone can build a broken Category
}
```

---

## 2. Business Rules via `CheckRule`

Every invariant is an `IBusinessRule` class. Aggregates and value objects validate state by calling `CheckRule(...)` on `Entity`/`ValueObject` base classes, which throws `BusinessRuleValidationException` when `IsBroken()` returns true. Domain code does **not** throw `InvalidOperationException`, `ArgumentException`, or any other framework exception. The only acceptable boundary exception is `TypedIdValueBase` rejecting `Guid.Empty` — programming-error guarding, not business-rule failure.

Why: a single exception type lets pipeline behaviors (`UnitOfWorkBehavior`, exception middleware) map domain failures to consistent 400-level responses without sniffing exception messages. Splitting business failures across multiple framework exceptions destroys that guarantee.

**DO**
```csharp
public void MakeStep(LinkId targetLinkId, ...)
{
    CheckRule(new GameMustBeInProgressRule(_gameState));
    CheckRule(new StepMustBeValidRule(_currentLinkId, targetLinkId, neighbors));
    // ...
}

public class GameMustBeInProgressRule : IBusinessRule
{
    private readonly GameState _state;
    public GameMustBeInProgressRule(GameState state) => _state = state;
    public bool IsBroken() => _state != GameState.InProgress
                           && _state != GameState.LastStepWarning;
    public string Message => "Game must be in progress.";
}
```

**DON'T**
```csharp
public void MakeStep(LinkId targetLinkId)
{
    if (_gameState != GameState.InProgress)
        throw new InvalidOperationException("Game not in progress."); // wrong exception type
}
```

---

## 3. Domain Events on Every Meaningful State Change

Whenever an aggregate transitions to a new meaningful state, it emits a `*DomainEvent` via `AddDomainEvent(...)`. Today the project has 17 events (10 Game + 5 Link + 2 Category). Adding a new state-mutating command on an aggregate is incomplete without its corresponding event.

Events serve two purposes: in-process side effects (handlers reacting to a state change) and the future Outbox/Inbox bridge to other modules. If a transition isn't worth notifying about, either it's not really a state change, or it's leaking implementation detail.

**DO**
```csharp
public void Start()
{
    CheckRule(new GameMustBeNotStartedRule(_gameState));
    _gameState = GameState.InProgress;
    AddDomainEvent(new GameStartedDomainEvent(Id));
}
```

**DON'T**
```csharp
public void Start()
{
    _gameState = GameState.InProgress;
    // missing event — no one downstream learns the game started
}
```

---

## 4. Cross-Aggregate References by Id Only

Aggregates reference each other through typed IDs (`CategoryId`, `LinkId`, `GameId` — all `TypedIdValueBase`). They never hold a reference to another aggregate's instance. This keeps each aggregate's transactional boundary intact and prevents lazy-loading graphs that drag entire object trees into memory.

When a domain operation needs data from another aggregate, the Application layer fetches it via that aggregate's repository and passes the data — or a domain service that abstracts the lookup — into the operation.

**DO**
```csharp
public class Game : Entity<GameId>, IAggregateRoot
{
    private CategoryId _categoryId;   // Id only
    private Puzzle _puzzle;           // owned VO, fine
    // ...
}
```

**DON'T**
```csharp
public class Game : Entity<GameId>, IAggregateRoot
{
    private Category _category;   // direct reference — breaks aggregate boundary
}
```

---

## 5. Domain Services as Method Parameters, Not Fields

When an aggregate needs a domain service (`IPathFinderService`, `ILinkNeighborResolver`, `IGameConfigurationService`, `IScoreCalculator`), it receives the service as a method parameter on the specific operation that needs it. Aggregates never store service references as fields. The Application layer resolves the service per call and hands it in.

This keeps aggregates serializable, persistable, and free of DI plumbing. It also makes which operations depend on which services explicit at the method signature — a glance at `Game.MakeStep(LinkId, ILinkNeighborResolver, IScoreCalculator)` tells you exactly what infrastructure that step touches.

**DO**
```csharp
public void MakeStep(LinkId target, ILinkNeighborResolver resolver, IScoreCalculator calc)
{
    var neighbors = resolver.GetOutgoingLinkIds(_currentLinkId);
    // ...
    if (target == _puzzle.TargetLinkId) Complete(calc);
}
```

**DON'T**
```csharp
public class Game
{
    private readonly ILinkNeighborResolver _resolver;   // service stored in aggregate
    public Game(ILinkNeighborResolver resolver) { _resolver = resolver; }
    public void MakeStep(LinkId target) { /* hidden dependency */ }
}
```

---

## 6. Value Objects are Immutable and Self-Validating

Value objects don't mutate; they return new instances. Allowance VOs (`HintAllowance`, `UndoAllowance`, `ResetAllowance`) demonstrate the canonical pattern: an `Of(int total)` factory creates the initial state, a `Consume()` method validates via `CheckRule` and returns the next state. The aggregate uses field reassignment: `_hintAllowance = _hintAllowance.Consume();`.

This pushes invariants into the VO itself, so the aggregate doesn't repeat allowance arithmetic, and it makes "have we run out?" a property of the VO rather than a magic number on the aggregate.

**DO**
```csharp
public sealed class HintAllowance : ValueObject
{
    public int Remaining { get; }
    public int Used { get; }
    public static HintAllowance Of(int total) => new(total, 0);
    public HintAllowance Consume()
    {
        CheckRule(new HintAllowanceMustHaveRemainingRule(Remaining));
        return new HintAllowance(Remaining - 1, Used + 1);
    }
}

// in Game:
_hintAllowance = _hintAllowance.Consume();   // reassignment, not mutation
```

**DON'T**
```csharp
private int _remainingHints;
public void UseHint()
{
    if (_remainingHints <= 0) throw new InvalidOperationException(); // primitive obsession
    _remainingHints--;                                                // mutating primitive
}
```

---

## 7. Cross-Aggregate Rules Live with the Consumer

Rules like `CategoryMustHaveEnoughLinksToStartGameRule` and `PuzzleTargetLinkMustBeReachableRule` involve both Category/Link state and Game/Puzzle creation. They live in `Modules/Games/Domain/Games/Rules/` — with the **consumer** (Game/Puzzle) — not with the source aggregate (Category/Link).

Why: those rules describe what the Game/Puzzle creation flow requires from its inputs. They aren't invariants of Category — a Category with three links is perfectly valid; it just can't seed a game. Putting the rule with the consumer keeps the source aggregate's invariant set focused.

**DO**
```csharp
// src/Modules/Games/Domain/Games/Rules/CategoryMustHaveEnoughLinksToStartGameRule.cs
public class CategoryMustHaveEnoughLinksToStartGameRule : IBusinessRule { ... }

// invoked in Puzzle.Create — the Game-creation flow
CheckRule(new CategoryMustHaveEnoughLinksToStartGameRule(category, links));
```

**DON'T**
```csharp
// src/Modules/Games/Domain/Categories/Rules/CategoryMustHaveEnoughLinks.cs
// — Category itself doesn't require any minimum link count
```

---

## 8. Pure Domain Logic Stays in Domain

Domain services with **no I/O** — `StandardScoreCalculator` for instance — live in the Domain project, not Infrastructure. The score formula (base = depth × 100, penalties for extra steps/hints/undos/resets, difficulty multiplier Easy=1.00 / Medium=1.15 / Hard=1.25) is pure arithmetic; pushing it to Infrastructure adds an indirection that buys nothing and forces Infrastructure dependencies during unit testing.

If a future calculator needs database lookups, configuration, or external data (e.g., a `ConfigurableScoreCalculator` that reads tunables from a config service), **that** one belongs in Infrastructure. The interface stays in Domain; the IO-bound implementation crosses the boundary.

**DO**
```csharp
// src/Modules/Games/Domain/Services/StandardScoreCalculator.cs — pure logic, no IO
public class StandardScoreCalculator : IScoreCalculator
{
    public int Calculate(Difficulty d, int targetDepth, int stepsTaken, int hints, int undos, int resets)
    {
        var basePoints = targetDepth * 100;
        var penalty = (stepsTaken - targetDepth) * 10 + hints * 20 + undos * 15 + resets * 40;
        var multiplier = d switch { Difficulty.Easy => 1.00, Difficulty.Medium => 1.15, _ => 1.25 };
        return (int)(Math.Max(0, basePoints - penalty) * multiplier);
    }
}
```

**DON'T**
```csharp
// src/Modules/Games/Infrastructure/Services/StandardScoreCalculator.cs
// — pure logic in Infrastructure means tests need to wire Infrastructure to compute a score
```

---

## 9. CQRS — Separate Read and Write Stacks

Write side: MediatR commands → handlers → repositories → aggregates. Aggregates enforce invariants; repositories persist whole aggregates via `IUnitOfWork.CommitAsync`.

Read side: MediatR queries → handlers → **Dapper** against database views (`[Games].[v_Categories]`, `[Games].[v_Links]`) → DTOs. Read handlers do not touch repositories or aggregates. They project view rows directly into positional `record` DTOs.

This split means the read model can be denormalized, view-shaped, or even materialized without dragging the write model along. It also keeps query handlers fast — no aggregate hydration cost when all the UI needs is a list.

**DO**
```csharp
// Read side — Dapper, view, DTO
internal class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, List<CategoryListItemDto>>
{
    public async Task<List<CategoryListItemDto>> Handle(GetCategoriesQuery q, CancellationToken ct)
    {
        var conn = _sqlConnectionFactory.GetOpenConnection();
        const string sql = """SELECT [Id], [Name] FROM [Games].[v_Categories] ORDER BY [Name]""";
        var rows = await conn.QueryAsync<CategoryListItemDto>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.AsList();
    }
}
```

**DON'T**
```csharp
// Read side — pulling aggregates just to project them
internal class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, List<CategoryListItemDto>>
{
    public async Task<List<CategoryListItemDto>> Handle(GetCategoriesQuery q, CancellationToken ct)
    {
        var categories = await _categoryRepository.GetAllAsync(ct);   // hydrates full aggregates
        return categories.Select(c => new CategoryListItemDto(c.Id.Value, c.Name)).ToList();
    }
}
```

---

## 10. Per-Module Contracts and Configuration

The CQRS contracts (`ICommand`, `ICommand<TResult>`, `IQuery<TResult>`, `ICommandHandler`, `IQueryHandler`) and pipeline configuration live in **each module's** `Application` project (`Modules/Games/Application/Configuration/`), not in `Common.Application`. This is Kamil Grzybek's deliberate choice: it keeps modules independently extractable into microservices later, lets each module evolve its pipeline behaviors without coordinating with other modules, and makes each module's contract surface self-contained.

If you find yourself wanting to "DRY up" the contracts into `Common.Application`, stop. Re-read this rule. The duplication is the point.

**DO**
```
src/Modules/Games/Application/Configuration/Commands/ICommand.cs
src/Modules/Games/Application/Configuration/Queries/IQueryHandler.cs
src/Modules/Players/Application/Configuration/Commands/ICommand.cs   // duplicated, on purpose
```

**DON'T**
```
src/Common/Application/Commands/ICommand.cs                          // shared = coupled
```

---

## 11. One Repository per Aggregate Root

Repositories exist only for aggregate roots — `ICategoryRepository`, `ILinkRepository`, `IGameRepository`. Child entities and value objects don't get repositories. The `IRepository<T>` marker enforces this with `where T : IAggregateRoot`.

Why: aggregates are the unit of consistency. A repository that loads a child entity in isolation lets callers mutate state outside the aggregate's invariants — exactly the boundary the aggregate exists to protect.

**DO**
```csharp
public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByIdAsync(CategoryId id, CancellationToken ct);
    Task AddAsync(Category category, CancellationToken ct);
}
```

**DON'T**
```csharp
public interface IPuzzleRepository : IRepository<Puzzle>   // Puzzle is a VO inside Game
public interface IAllowanceRepository                      // child VO — no repository
```

---

## 12. No Hard Delete — Soft Delete via State Methods

Aggregates do not expose `Delete()` or `Remove()`. Lifecycle changes go through explicit state methods backed by an `IsActive` flag, rules, and events: `Activate()` / `Deactivate()` on `Link`, with `LinkMustBeInactiveToActivateRule` / `LinkMustBeActiveToDeactivateRule` and `LinkActivatedDomainEvent` / `LinkDeactivatedDomainEvent`.

Hard delete is inappropriate for this domain because past Games reference Links by Id and need their words to remain readable for history/replay. Deactivation is also a state transition worth notifying about — same as any other.

**DO**
```csharp
public void Deactivate()
{
    CheckRule(new LinkMustBeActiveToDeactivateRule(_isActive));
    _isActive = false;
    AddDomainEvent(new LinkDeactivatedDomainEvent(Id));
}
```

**DON'T**
```csharp
public void Delete()                      // no hard delete on aggregates
{
    AddDomainEvent(new LinkDeletedEvent(Id));
}

// or in a repository:
Task RemoveAsync(LinkId id);              // no — repositories don't expose hard delete
```

---

## 13. Module Isolation for Microservice Extraction

Modules don't share code at the `Application` or `Infrastructure` layer. Cross-module communication, when it eventually arrives (Player module is the first candidate), goes through Integration Events published via an Outbox — never through direct method calls or shared services.

This rule is the one that stays untested until the second module appears. Treat every "shortcut" that crosses module boundaries with suspicion. If you can't extract the module into its own service tomorrow without rewriting that shortcut, you've broken this rule.

**DO**
```csharp
// Games module raises a domain event; an outbox publishes a GameCompletedIntegrationEvent.
// The Player module subscribes through its Inbox and updates streak/rank independently.
```

**DON'T**
```csharp
// In Games.Application:
public class GameCompletedHandler
{
    private readonly IPlayerStreakService _streak; // direct cross-module call
    public Task Handle(GameCompletedDomainEvent e) => _streak.IncrementStreakAsync(e.PlayerId);
}
```

---

## 14. Public Contracts, Internal Implementations

Within a module's Application project, **commands, queries, DTOs, and IDs are `public`**; **handlers, factories, and the aggregate's `Create` methods are `internal`**. Only the contract surface leaves the module assembly. Implementation details — including how a command is handled — stay encapsulated.

The same applies in Domain: `Entity`, `IAggregateRoot`, the typed IDs, and `*DomainEvent` types are `public`; aggregate constructors and factories are `private`/`internal`. Outside code obtains aggregates through repositories, not by calling factories directly.

**DO**
```csharp
public class CreateCategoryCommand : CommandBase<Guid> { /* contract */ }
internal class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, Guid> { ... }
public record CategoryListItemDto(Guid Id, string Name);
```

**DON'T**
```csharp
public class CreateCategoryCommandHandler : ...   // handler is now part of the public API
internal class CreateCategoryCommand : ...        // command unreachable from outside the module
```

---

## 15. Don't Suppress CS8618 on EF Core Constructors

Parameterless EF Core constructors will generate CS8618 warnings (non-nullable field uninitialized). These are expected. **Don't** silence them with `null!`, `#pragma warning disable`, or by making fields nullable — those workarounds either lie to the type system or change the runtime model.

Sprint 3 will resolve every CS8618 warning by configuring backing-field access through Fluent API (`UsePropertyAccessMode(PropertyAccessMode.Field)`) plus owned-type configuration for VOs. Until then, the warnings are part of the agreed transitional state.

**DO**
```csharp
private Game() { }   // EF Core ctor; CS8618 warnings on the non-nullable fields are fine
```

**DON'T**
```csharp
private Game()
{
    _puzzle = null!;                // lying to the compiler
    _hintAllowance = null!;
}

// or:
private Puzzle? _puzzle;            // making the field nullable to silence CS8618
```

---

## See Also

- `GLOSSARY.md` — what each domain term means.
- `CONVENTIONS.md` — code conventions (naming, visibility, file layout) that complement these rules.
- `ROADMAP.md` — sprint plan and current focus.
- `activeContext.md` — recent design choices and surprises that don't fit the rule format above.
