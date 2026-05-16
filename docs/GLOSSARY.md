# GLOSSARY.md

Ubiquitous language for the LexiLink Games, Players, Energy, and Quests modules. Every term below appears in code as a class, interface, enum, or namespace; this file gives it a one-paragraph definition so a new contributor can read the code without spelunking.

---

## Aggregates

### Category — `Modules/Games/Domain/Categories/Category.cs`
A named set of words from which a game can be seeded (e.g., *animals*, *colors*). Aggregate root. Holds `_name` (≤ 100 chars, non-empty) and an optional `_description` (≤ 500 chars). Lifecycle: created via `Category.Create(name, description)`; renamed via `EditGeneralInfo(...)`. Emits `CategoryCreatedDomainEvent` and `CategoryEditedDomainEvent`.

### Link — `Modules/Games/Domain/Links/Link.cs`
A word node in the directed word-graph. Aggregate root. Holds `_categoryId` (the Category the word belongs to), `_value` (the word itself), `_description`, `_isActive`, and `_outgoingLinks` (a list of `LinkId`s pointing to neighbor words). Content (`_value`, `_description`) is **immutable** — to change a word's text, deactivate the old Link and create a new one. Outgoing topology *is* mutable through `AddOutgoingLink` / `RemoveOutgoingLink` (graph-relationship management). Activation flips through `Activate()` / `Deactivate()`.

### Game — `Modules/Games/Domain/Games/Game.cs`
A single player's session solving one Puzzle. Aggregate root. The most complex aggregate in the module. Owns a `Puzzle` value object, three `*Allowance` VOs, an optional `Score`, a `_history` of `LinkId`s walked, a `StepBudget`, and a `GameState`. Drives an explicit state machine — see *Game States* below.

### PlayerEnergy — `Modules/Energy/Domain/PlayerEnergies/PlayerEnergy.cs`
A player's energy account in the Energy module. Aggregate root, identified by `PlayerEnergyId` (same Guid value as the owning `PlayerId` — cross-module reference by id only). Holds `_currentAmount`, `_maximumAmount`, `_rechargeIntervalSeconds`, and `_lastRefilledOn`. Lifecycle: `InitializeFor(playerId, max, intervalSeconds, initializedAt)` (called by `EnsurePlayerEnergyExistsCommand` after a `PlayerRegisteredIntegrationEvent`). Behavior: `Consume(amount, now)` recharges first via `RechargeBasedOnElapsedTime(now)`, then checks `EnergyMustBeSufficientToConsumeRule`, then debits; the refill timer is armed **only when consume crosses the bucket from at/above max to below max** (so 10/5 → 9/5 and 6/5 → 5/5 leave the timer idle, while 5/5 → 4/5 sets it). `GrantBonus(amount, now)` adds `amount` without checking max — it is the reward path invoked by `QuestClaimedIntegrationEvent`; over-max balance is preserved and the timer stays idle until the bucket drains back below max. Emits `PlayerEnergyConsumedDomainEvent` and `PlayerEnergyRefilledDomainEvent`.

### PlayerQuest — `Modules/Quests/Domain/PlayerQuests/PlayerQuest.cs`
A single quest assignment to a single player. Aggregate root, identified by `PlayerQuestId` (its own Guid, unrelated to `PlayerId`). Holds `_playerId`, `_questType`, `_progress`, `_goal`, `_rewardAmount`, `_state`, `_issuedAt`, `_completedAt`, `_claimedAt`, and `_expiresAt`. Lifecycle: `IssueFor(playerId, questType, goal, rewardAmount, issuedAt, expiresAt?)` (creates in `Active` state). Behavior: `RecordProgress(delta, now)` first calls `ExpireIfPast(now)`, then advances `_progress` (clamped at `_goal`); when progress reaches `_goal` the state flips to `ReadyToClaim`. `Claim(now)` calls `ExpireIfPast(now)`, then flips `ReadyToClaim → Claimed`. `ExpireIfPast(now)` transitions `Active`/`ReadyToClaim` → `Expired` when `_expiresAt` has passed. Emits `PlayerQuestIssuedDomainEvent`, `PlayerQuestCompletedDomainEvent`, `PlayerQuestClaimedDomainEvent`.

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

### QuestType — `Quests/PlayerQuests/QuestType.cs`
Enum identifying which quest. MVP catalog: `FirstGameCompleted`, `ThreeGamesCompleted`, `AccountLinked`, `DailyThreeGames`. Persisted to `quests.PlayerQuests.QuestType` as `varchar(64)` via EF `HasConversion<string>()` so DB rows stay self-describing.

### QuestState — `Quests/PlayerQuests/QuestState.cs`
Enum: `Active`, `ReadyToClaim`, `Claimed`, `Expired`. State machine: `Active → ReadyToClaim` on progress completion; `ReadyToClaim → Claimed` on explicit `Claim()` (player taps claim); `Active`/`ReadyToClaim → Expired` lazily via `ExpireIfPast(now)`. Persisted as `varchar(32)` via `HasConversion<string>()`.

### QuestCadence — `Quests/PlayerQuests/QuestCadence.cs`
Enum: `OneTime` or `Daily`. Used in `QuestDefinition` to decide whether `IssueQuestCommand` should set `_expiresAt = NextUtcMidnight(now)` (Daily) or leave it null (OneTime). Daily expiry is checked lazily on read/progress; the next event after expiry re-issues a fresh daily quest.

### QuestDefinition — `Quests/PlayerQuests/QuestDefinition.cs`
Sealed record `QuestDefinition(QuestType, QuestCadence, int Goal, int RewardAmount, QuestType? PrerequisiteQuestType)`. The catalog entry for one quest. `PrerequisiteQuestType` gates issuance — `AccountLinked` requires `ThreeGamesCompleted` to be **claimed** before it can be issued. MVP catalog: `FirstGameCompleted` (OneTime, 1, +3⚡), `ThreeGamesCompleted` (OneTime, 3, +5⚡), `AccountLinked` (OneTime, 1, +5⚡, prereq=ThreeGamesCompleted), `DailyThreeGames` (Daily, 3, +5⚡).

### HintAllowance, UndoAllowance, ResetAllowance — `Games/Allowances/`
Three immutable VOs sharing the same shape: `Of(int total)` factory, `Remaining` and `Used` getters, `Consume()` returning a new instance after `CheckRule(*MustHaveRemainingRule)`. Three separate types instead of one generic — semantic distinction is preserved in the type system. Game uses field reassignment: `_hintAllowance = _hintAllowance.Consume();`.

### StepBudget — `Games/StepBudget.cs`
Encapsulates `Max` + `Taken` plus the `Step()`, `UndoStep()`, `Reset()` methods and the `IsExhausted` / `IsAtLastWarning` / `IsBelowLastWarning` predicates. Replaced loose `_maxSteps` int + `history.Count` arithmetic that used to live across `Game.cs`.

### EnergyRefillCalculator — `Modules/Energy/Domain/PlayerEnergies/EnergyRefillProjection.cs`
`internal static` pure-math helper. `Project(current, max, lastRefilledOn, intervalSeconds, now)` returns an `EnergyRefillProjection(currentAmount, lastRefilledOn)` after applying time-elapsed refills. Capped at max; partial intervals are preserved (the leftover seconds carry into the next refill). Used by both `PlayerEnergy.RechargeBasedOnElapsedTime` (write path) and `GetPlayerEnergyQueryHandler` (read path, via `InternalsVisibleTo`) so the math has one source of truth.

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

## Domain Events (20 total)

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

### PlayerEnergy (2) — `Energy/Domain/PlayerEnergies/Events/`
- `PlayerEnergyConsumedDomainEvent` — emitted by `Consume()` with `PlayerId`, `Amount`, `RemainingAmount`.
- `PlayerEnergyRefilledDomainEvent` — emitted by `RechargeBasedOnElapsedTime()` only when at least one tick is gained; carries `PlayerId`, `GainedAmount`, `CurrentAmount`.

### PlayerQuest (3) — `Quests/Domain/PlayerQuests/Events/`
- `PlayerQuestIssuedDomainEvent` — emitted by `IssueFor(...)`; carries `PlayerQuestId`, `PlayerId`, `QuestType`.
- `PlayerQuestCompletedDomainEvent` — emitted by `RecordProgress(...)` when progress reaches goal.
- `PlayerQuestClaimedDomainEvent` — emitted by `Claim(now)`; mapped to `QuestClaimedIntegrationEvent` via outbox so Energy can grant the reward.

All events derive from `DomainEvent` (`Common/Domain/DomainEvent.cs`) which extends `IDomainEvent : INotification` — so they're MediatR-compatible without further wiring.

---

## Business Rules (24 total)

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

### PlayerEnergy (5) — `Energy/Domain/PlayerEnergies/Rules/`
- `EnergyConfigurationMustBeValidRule` — `maxAmount > 0` and `rechargeIntervalSeconds > 0` at initialization.
- `EnergyAmountCannotBeNegativeRule` — `Consume(amount)` must receive `amount >= 0`.
- `EnergyAmountCannotExceedMaximumRule` — invariant guard (defensive; not enforced on `Consume`/`GrantBonus`, which deliberately permit over-max via the bonus path).
- `EnergyMustBeSufficientToConsumeRule` — `_currentAmount >= requestedAmount`; the rule whose violation propagates as `BusinessRuleValidationException` and is what blocks a `StartGameCommand` when energy is empty.
- `BonusAmountMustBePositiveRule` — `GrantBonus(amount, now)` requires `amount > 0` (zero or negative bonuses are nonsensical).

### PlayerQuest (5) — `Quests/Domain/PlayerQuests/Rules/`
- `QuestGoalMustBePositiveRule` — `IssueFor(...)` requires `goal > 0`.
- `QuestRewardAmountMustBePositiveRule` — `IssueFor(...)` requires `rewardAmount > 0`.
- `QuestProgressDeltaMustBePositiveRule` — `RecordProgress(delta, now)` requires `delta > 0`.
- `QuestMustBeActiveToProgressRule` — `RecordProgress` requires `_state == Active`.
- `QuestMustBeReadyToBeClaimedRule` — `Claim(now)` requires `_state == ReadyToClaim`.

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

`IEnergyConfigurationService` (`Modules/Energy/Domain/PlayerEnergies/`) exposes `MaximumAmount`, `RechargeIntervalSeconds`, and `GameStartCost`. Read from `IConfiguration` keys `Energy:MaxAmount`, `Energy:RechargeIntervalSeconds`, and `Energy:GameStartCost` with safe defaults (5 / 900 / 1).

`IQuestCatalog` (Quests domain service surface; implementation lives in Quests.Infrastructure) — see Cross-Module Gateways section.

---

## Cross-Module Gateways

### IEnergyGuard — `Modules/Games/Application/Configuration/CrossModule/IEnergyGuard.cs`
The first synchronous cross-module gateway in LexiLink. Contract lives in **Games.Application** so Games depends only on its own surface (`EnsureCanStartGameAsync(playerId, ct)`). The adapter (`LexiLink.API/CrossModule/EnergyGuard.cs`) is composed in the API host and translates the call into `IEnergyModule.ExecuteCommandAsync(new ConsumePlayerEnergyCommand(playerId, _energyConfiguration.GameStartCost))`. `StartGameCommandHandler` invokes the guard **before** `game.Start()`: insufficient energy throws `BusinessRuleValidationException` and the game state never advances from `Initial`. Residual dual-write risk (energy debited but `game.Start()` throws on a duplicate call) is accepted for MVP. Documented as the intentional deviation in `kamil-modular-monolith-comparison.md`; architecture tests forbid Games.Application from depending on any Energy namespace.

### IQuestCatalog — `Modules/Quests/Domain/PlayerQuests/IQuestCatalog.cs`
Resolves a `QuestType` to its `QuestDefinition`. Implementation in Quests.Infrastructure (`QuestCatalog`) is hardcoded for the MVP; future iterations may load from configuration or a content service. Registered as a singleton in `QuestsAutofacModule`.

### QuestClaimedIntegrationEvent — `Modules/Quests/IntegrationEvents/QuestClaimedIntegrationEvent.cs`
Public contract emitted by Quests' outbox after `PlayerQuestClaimedDomainEvent` lands. Carries `PlayerId`, `PlayerQuestId`, `QuestType` (string), and `RewardAmount`. Consumed by Energy.Application's `QuestClaimedIntegrationEventHandler`, which idempotently ensures the energy aggregate exists and then dispatches `GrantEnergyCommand`. This is LexiLink's **first reverse cross-module event dependency**: Energy.Application references `Quests.IntegrationEvents` (granular allow), analogous to how Stats references Games/Players IntegrationEvents. ArchTests enforce that the dependency stays public-contract-only — Quests.Domain/Application/Infrastructure remain forbidden from any consumer module.

---

## Repositories

| Interface | Aggregate | Methods |
| --- | --- | --- |
| `ICategoryRepository` | `Category` | `GetByIdAsync`, `AddAsync` |
| `ILinkRepository` | `Link` | `GetByIdAsync`, `GetIdsByCategoryAsync`, `AddAsync` |
| `IGameRepository` | `Game` | `GetByIdAsync`, `AddAsync` |
| `IPlayerEnergyRepository` | `PlayerEnergy` | `GetByIdAsync`, `AddAsync` |
| `IPlayerQuestRepository` | `PlayerQuest` | `GetByIdAsync`, `GetActiveOrReadyByPlayerAndTypeAsync`, `GetByPlayerAsync`, `HasClaimedAsync`, `AddAsync` |

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
