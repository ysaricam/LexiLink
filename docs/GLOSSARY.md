# GLOSSARY.md

Ubiquitous language for the LexiLink Games module. Every term below appears in code as a class, interface, enum, or namespace; this file gives it a one-paragraph definition so a new contributor can read the code without spelunking.

---

## Aggregates

### Category — `Modules/Games/Domain/Categories/Category.cs`
A named set of words from which a game can be seeded (e.g., *animals*, *colors*). Aggregate root. Holds `_name` (≤ 100 chars, non-empty) and an optional `_description` (≤ 500 chars). Lifecycle: created via `Category.Create(name, description)`; renamed via `EditGeneralInfo(...)`. Emits `CategoryCreatedDomainEvent` and `CategoryEditedDomainEvent`.

### Link — `Modules/Games/Domain/Links/Link.cs`
A word node in the directed word-graph. Aggregate root. Holds `_categoryId` (the Category the word belongs to), `_value` (the word itself), `_description`, `_isActive`, and `_outgoingLinks` (a list of `LinkId`s pointing to neighbor words). Content (`_value`, `_description`) is **immutable** — to change a word's text, deactivate the old Link and create a new one. Outgoing topology *is* mutable through `AddOutgoingLink` / `RemoveOutgoingLink` (graph-relationship management). Activation flips through `Activate()` / `Deactivate()`.

### Game — `Modules/Games/Domain/Games/Game.cs`
A single player's session solving one Puzzle. Aggregate root. The most complex aggregate in the module. Owns a `Puzzle` value object, three `*Allowance` VOs, an optional `Score`, a `_history` of `LinkId`s walked, a `StepBudget`, and a `GameState`. Drives an explicit state machine — see *Game States* below.

---

## Value Objects

### Puzzle — `Games/Puzzles/Puzzle.cs`
The board: a category, a difficulty, a start `LinkId`, a target `LinkId`, and the optimal path between them (computed once at creation by `IPathFinderService`). Sealed VO inside `Game`. Created via `Puzzle.Create(...)` which receives `IPathFinderService`, `IGameConfigurationService`, and a `Random` — never stores them. Exposes `RequestHint(currentLinkId)` returning a `HintResult`.

### HintResult — `Games/Puzzles/HintResult.cs`
What a player gets when they spend a hint: a flag indicating whether their current position is on the optimal path, plus the next correct link if so.

### Score — `Games/Score.cs`
The points a Game ends with. Pure VO with a single `Points` getter and a `Score.Of(int)` factory. Computed by `IScoreCalculator` at completion — `Score` itself does no arithmetic.

### Difficulty — `Games/Difficulty.cs`
Enum: `Easy`, `Medium`, `Hard`. Drives `IGameConfigurationService.ResolveDepthRange`, the score multiplier (Easy=1.00, Medium=1.15, Hard=1.25), and step/allowance budgets.

### GameState — `Games/GameState.cs`
Enum: `Initial`, `InProgress`, `LastStepWarning`, `Completed`, `Failed`, `Abandoned`. Drives the state machine — see below.

### HintAllowance, UndoAllowance, ResetAllowance — `Games/Allowances/`
Three immutable VOs sharing the same shape: `Of(int total)` factory, `Remaining` and `Used` getters, `Consume()` returning a new instance after `CheckRule(*MustHaveRemainingRule)`. Three separate types instead of one generic — semantic distinction is preserved in the type system. Game uses field reassignment: `_hintAllowance = _hintAllowance.Consume();`.

### StepBudget — `Games/StepBudget.cs`
Encapsulates `Max` + `Taken` plus the `Step()`, `UndoStep()`, `Reset()` methods and the `IsExhausted` / `IsAtLastWarning` / `IsBelowLastWarning` predicates. Replaced loose `_maxSteps` int + `history.Count` arithmetic that used to live across `Game.cs`.

---

## Game States — transitions

```
                     Start()
        Initial ──────────────► InProgress
                                    │
                  history.Count == max-1
                                    ▼
                              LastStepWarning ── Undo() (if budget recovers) ──► InProgress
                                    │
                          target reached       step taken at limit
                                    ▼                  ▼
                              Completed             Failed

        InProgress / LastStepWarning ── Abandon() ──► Abandoned
```

Every transition is guarded by `CheckRule(...)` and emits a `*DomainEvent`. `Initial → InProgress` requires `GameMustBeNotStartedRule`; most other transitions require `GameMustBeInProgressRule`.

---

## Domain Events (17 total)

### Game (10) — `Games/Events/`
- `GameCreatedDomainEvent` — game session created (Initial state).
- `GameStartedDomainEvent` — `Start()` succeeded; entered InProgress.
- `StepMadeDomainEvent` — player walked from one Link to a neighbor.
- `LastStepWarningIssuedDomainEvent` — one step away from Failed.
- `HintUsedDomainEvent` — hint allowance consumed; carries the `HintResult`.
- `UndoUsedDomainEvent` — last step rolled back.
- `ResetUsedDomainEvent` — game rewound to start.
- `GameCompletedDomainEvent` — target reached; `Score` computed.
- `GameFailedDomainEvent` — step budget exhausted without reaching target.
- `GameAbandonedDomainEvent` — player gave up.

### Link (5) — `Links/Events/`
- `LinkCreatedDomainEvent` — new word added.
- `OutgoingLinkAddedDomainEvent` — graph edge added.
- `OutgoingLinkRemovedDomainEvent` — graph edge removed.
- `LinkActivatedDomainEvent` — soft-deactivated word brought back.
- `LinkDeactivatedDomainEvent` — soft-deletion.

### Category (2) — `Categories/Events/`
- `CategoryCreatedDomainEvent`
- `CategoryEditedDomainEvent` — name and/or description changed.

All events derive from `DomainEvent` (`Common/Domain/DomainEvent.cs`) which extends `IDomainEvent : INotification` — so they're MediatR-compatible without further wiring.

---

## Business Rules (18 total)

### Category (3) — `Categories/Rules/`
- `CategoryNameMustNotBeEmptyRule`
- `CategoryNameMustNotExceedMaxLengthRule` (≤ 100)
- `CategoryDescriptionMustNotExceedMaxLengthRule` (≤ 500, null OK)

### Link (5) — `Links/Rules/`
- `LinkCannotPointToItselfRule` — no self-loops.
- `LinkOutgoingAlreadyExistsRule` — can't add an edge that already exists.
- `LinkOutgoingMustExistRule` — can't remove an edge that doesn't exist.
- `LinkMustBeInactiveToActivateRule`
- `LinkMustBeActiveToDeactivateRule`

### Game (7) — `Games/Rules/`
- `GameMustBeNotStartedRule` — `Start()` only from Initial.
- `GameMustBeInProgressRule` — `MakeStep`, `UseHint`, `Undo`, `ResetToStart` require InProgress (or LastStepWarning where applicable).
- `GameMustNotBeFinishedRule` — `Abandon` only from active states.
- `StepMustBeValidRule` — target Link must be a neighbor of current Link.
- `GameHistoryMustNotBeEmptyRule` — `Undo` and `ResetToStart` require at least one prior step.
- `CategoryMustHaveEnoughLinksToStartGameRule` — Puzzle creation invariant.
- `PuzzleTargetLinkMustBeReachableRule` — Puzzle creation invariant.

### Allowance (3) — `Games/Allowances/Rules/`
- `HintAllowanceMustHaveRemainingRule`
- `UndoAllowanceMustHaveRemainingRule`
- `ResetAllowanceMustHaveRemainingRule`

All are `IBusinessRule` and dispatch through `CheckRule(...)`, throwing `BusinessRuleValidationException` on failure.

---

## Domain Services

| Interface | Purpose | Implementation lives in |
| --- | --- | --- |
| `IPathFinderService` (`Services/`) | Find optimal path from start to target Link; pick a valid target at a given depth. | Infrastructure (Sprint 3) — graph traversal (BFS/Dijkstra). |
| `ILinkNeighborResolver` (`Services/`) | Look up `LinkId`s adjacent to a given `LinkId`. | Infrastructure (Sprint 3) — wraps `ILinkRepository`. |
| `IGameConfigurationService` (`Services/`) | Resolve depth range, step budget, and allowance counts for a `Difficulty`. | Infrastructure (Sprint 3) — config-driven. |
| `IScoreCalculator` (`Services/`) | Compute final `Score` from depth + step/hint/undo/reset counts + difficulty. | **Domain** (`StandardScoreCalculator`) — pure logic, no I/O. A future `ConfigurableScoreCalculator` would live in Infrastructure. |

Domain services are received as method parameters — never stored on aggregates. See SKILLS.md rule #5.

---

## Repositories

| Interface | Aggregate | Methods |
| --- | --- | --- |
| `ICategoryRepository` | `Category` | `GetByIdAsync`, `AddAsync` |
| `ILinkRepository` | `Link` | `GetByIdAsync`, `GetIdsByCategoryAsync`, `AddAsync` |
| `IGameRepository` | `Game` | `GetByIdAsync`, `AddAsync` |

No `Update` / `Delete` methods — aggregates mutate in place; `IUnitOfWork.CommitAsync()` persists changes through EF Core's change tracker. Soft-delete uses state methods on the aggregate (see SKILLS.md rule #12).

---

## Application Concepts

### Command / Query
Write-side requests (`CreateLinkCommand`, `EditCategoryCommand`) implement `ICommand` or `ICommand<TResult>`. Read-side requests (`GetCategoriesQuery`, `GetLinkDetailsQuery`) extend `QueryBase<TResult>` (which carries `Id` for pipeline correlation) and implement `IQuery<TResult>`. Both contracts live in `Modules/Games/Application/Configuration/{Commands,Queries}/` — per-module, intentionally not in `Common.Application`.

### Handler
`ICommandHandler<TCommand>` / `ICommandHandler<TCommand, TResult>` and `IQueryHandler<TQuery, TResult>`. Handlers are `internal` — only the contract surface escapes the module assembly.

### DTO
Read-side projection target. Always a positional `record` (works with Dapper, immutable, modern C# affordance). One DTO per query — feature folder `Application/<Aggregate>/<QueryName>/` owns its DTO file.

### Pipeline Behavior (planned, Sprint 2)
MediatR `IPipelineBehavior<TRequest, TResponse>` implementations: `LoggingBehavior` (uses `QueryBase.Id` for correlation), `ValidationBehavior` (FluentValidation), `UnitOfWorkBehavior` (wraps commands in `IUnitOfWork.CommitAsync`).

### Repository, UnitOfWork
Repositories load and add aggregates; `IUnitOfWork.CommitAsync(CancellationToken)` flushes pending changes through EF Core. The `UnitOfWorkBehavior` calls `CommitAsync` after a successful command handler returns.

### NotFoundException — `Common/Application/Exceptions/`
Thrown by handlers when a `GetByIdAsync` or `QuerySingleOrDefaultAsync` returns `null`. Carries `EntityName` + `Id`. Mapped to HTTP 404 by exception middleware (Sprint 4).

---

## See Also

- `SKILLS.md` — the rules these terms participate in.
- `CONVENTIONS.md` — naming and code-shape conventions for these terms.
- `ROADMAP.md` — when each remaining piece (Application/Infrastructure/API/Tests) lands.
