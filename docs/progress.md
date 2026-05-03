# progress.md

History log of delivered work. Newest at top. Append entries when significant work lands; don't rewrite the past.

---

## Sprint 3 — Games.Infrastructure (in progress, started 2026-05-03)

### `Modules/Games/Infrastructure/Domain/Links/` — Link aggregate (slice 2, 2026-05-03)

- `LinkEntityTypeConfiguration : IEntityTypeConfiguration<Link>` — `internal`. Maps to table `Links` in schema `games`. `HasKey(x => x.Id)` (typed-ID auto-converted by selector). Backing fields `_value` (column `Value`), `_description` (column `Description`), `_isActive` (column `IsActive`), `_categoryId` (column `CategoryId`). Owned collection mapping for `_outgoingLinks` via `OwnsMany<OutgoingLink>("_outgoingLinks", ...)` → table `LinkOutgoingLinks` in schema `games`, FK column `LinkId`, payload column `OutgoingLinkId`, composite PK `(LinkId, OutgoingLinkId)`. `Navigation("_outgoingLinks").UsePropertyAccessMode(PropertyAccessMode.Field)` to be explicit about field access. Column names match `[Games].[LinkOutgoingLinks]` already referenced by `GetLinkOutgoingLinksQueryHandler`.
- `LinkRepository : ILinkRepository` — `internal`, injects `GamesContext`. Methods:
  - `GetByIdAsync` — `FirstOrDefaultAsync(x => x.Id == id)`; owned collection auto-loaded by EF, no explicit `Include`.
  - `GetIdsByCategoryAsync` — `EF.Property<CategoryId>(x, "_categoryId") == categoryId` projection to `Id`.
  - `GetActiveIdsByCategoryAsync` — adds `EF.Property<bool>(x, "_isActive")` filter.
  - `AddAsync` — `Links.AddAsync`. **No `Commit()` method** — UnitOfWorkCommandHandlerDecorator handles it.

### Domain additions (driven by Link mapping)

- New VO `OutgoingLink` (`Modules/Games/Domain/Links/OutgoingLink.cs`) — `sealed class : ValueObject`, single property `LinkId TargetId`. `internal` ctor + EF parameterless ctor. Wraps the foreign-Id reference so EF has an entity-shaped target for `OwnsMany`, and adds semantic clarity (it's an "outgoing edge", not a stray LinkId).
- `Link._outgoingLinks` field type `List<LinkId>` → `List<OutgoingLink>`. `AddOutgoingLink` wraps; `RemoveOutgoingLink` `RemoveAll(o => o.TargetId == ...)`. Domain event payloads (`OutgoingLinkAddedDomainEvent`, `OutgoingLinkRemovedDomainEvent`) unchanged — still carry `LinkId`.
- Two rules updated to `IReadOnlyCollection<OutgoingLink>` and `.Any(o => o.TargetId == ...)`: `LinkOutgoingAlreadyExistsRule`, `LinkOutgoingMustExistRule`.

### `Common.Infrastructure` — EF Core building blocks (Kamil-faithful)

- `IUnitOfWork` moved from `Common.Domain` → `Common.Infrastructure` to match Kamil's BuildingBlocks/Infrastructure placement. Signature unchanged for now (`Task<int> CommitAsync(CancellationToken ct = default)`); the optional `Guid? internalCommandId` parameter Kamil has is deferred until InternalCommands infra ships (Beyond Sprint 5). Two consumers updated: `UnitOfWorkCommandHandlerDecorator<T>` and `UnitOfWorkCommandHandlerWithResultDecorator<T, TResult>` `using` lines now point to `LexiLink.Common.Infrastructure`.
- `UnitOfWork : IUnitOfWork` — injects generic `DbContext` + `IDomainEventsDispatcher`; `CommitAsync` first dispatches domain events, then `_context.SaveChangesAsync(ct)`. Birebir Kamil. Composition root binds the abstract `DbContext` to the per-module concrete (`GamesContext`).
- `TypedIdValueConverter<TTypedIdValue> : ValueConverter<TTypedIdValue, Guid>` — Andrew Lock's strongly-typed-ID pattern; `id => id.Value` for serialize, `value => Activator.CreateInstance(typeof(TTypedIdValue), value)` for deserialize.
- `StronglyTypedIdValueConverterSelector : ValueConverterSelector` — auto-applies `TypedIdValueConverter<>` to any property whose CLR type derives from `TypedIdValueBase`. `ConcurrentDictionary` cache.

### `Modules/Games/Infrastructure/` — DbContext + first aggregate

- `GamesContext : DbContext` at the module Infrastructure root. `DbSet<Category>`, `DbSet<Link>`, `DbSet<Game>`. Ctor takes `(DbContextOptions, ILoggerFactory)` (matches Kamil's `MeetingsContext` signature). `OnModelCreating` does `ApplyConfigurationsFromAssembly(GetType().Assembly)` — entity configurations auto-scanned from the assembly.
- `Domain/Categories/CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>` — `internal`. Maps to table `Categories` in schema `games` (lowercase per Postgres convention). `HasKey(x => x.Id)` (typed-ID auto-converted by selector). Backing fields `_name` (column `Name`) and `_description` (column `Description`).
- `Domain/Categories/CategoryRepository : ICategoryRepository` — `internal`, injects `GamesContext` (concrete, not generic `DbContext`). Methods: `GetByIdAsync`, `AddAsync`. **No `Commit()` method** — the UnitOfWorkCommandHandlerDecorator in the pipeline calls `IUnitOfWork.CommitAsync` after the handler runs.
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1` added to `LexiLink.Modules.Games.Infrastructure.csproj`.

---

## Sprint 2 — Application Layer (closed 2026-05-03)

### `Common.Infrastructure` — Domain event dispatch

- `DomainEventsAccessor` — pulls domain events from EF Core `ChangeTracker` (`Entries<Entity>()`), returns flattened `IReadOnlyCollection<IDomainEvent>`; `ClearAllDomainEvents` resets per-aggregate buckets after dispatch.
- `DomainEventsDispatcher` — Autofac scope resolves an `IDomainEventNotification<>` per concrete event type, MediatR publishes the events in-process, then notifications are JSON-serialized (`AllPropertiesContractResolver`) into `OutboxMessage`s.
- `DomainEventsDispatcherNotificationHandlerDecorator<T>` — wraps every `INotificationHandler<T>` so cascade events from a handler also get dispatched.
- `DomainNotificationsMapper` + `BiDictionary<string, Type>` — bidirectional map between outbox `Type` strings and runtime notification types.

### `Modules/Games/Infrastructure/Configuration/Processing/` — Command-handler decorators (Kamil-style)

- `UnitOfWorkCommandHandlerDecorator<T>` — void variant; commits `IUnitOfWork` after the inner `ICommandHandler<T>` returns. `where T : ICommand`.
- `UnitOfWorkCommandHandlerWithResultDecorator<T, TResult>` — result variant for `CreateGame : CommandBase<Guid>`, `UseHint : CommandBase<HintResultDto>`. `where T : ICommand<TResult>`.
- `LoggingCommandHandlerDecorator<T>` + `LoggingCommandHandlerWithResultDecorator<T, TResult>` — Serilog-based, Kamil-faithful: `LogContext.Push(new CommandLogEnricher(command))` adds `"Context" = "Command:{Id}"` to every log line; try/catch logs `Information` on success and `Error` on exception (then rethrows). Result variant uses `{@Command}` destructuring for the entry log and logs the result on success.
  - Deliberately **omitted vs Kamil**: `RequestLogEnricher` + `IExecutionContextAccessor` ctor parameter (no HTTP host yet) and `if (command is IRecurringCommand) { … }` short-circuit (no InternalCommands infra yet). Both pluggable later without restructuring.
- `ValidationCommandHandlerDecorator<T>` + `ValidationCommandHandlerWithResultDecorator<T, TResult>` — FluentValidation, Kamil-faithful: ctor takes `IList<IValidator<T>>`, runs `.Validate(command)` per validator, flattens `Errors`, throws `InvalidCommandException(List<string> Errors)` if any error surfaces; otherwise delegates to inner handler. Result variant returns the inner `Task<TResult>` directly (no `await`) — matches Kamil byte-for-byte.
- All `internal` (Autofac decorator pattern; only the container constructs them).
- Generic constraint targets Games' own `ICommand` (from `Games.Application.Contracts`), not MediatR's `IRequest` directly — matches Kamil's per-module Processing/ layout exactly.
- Packages added to `LexiLink.Modules.Games.Infrastructure.csproj`: `Serilog 4.3.1`, `FluentValidation 12.1.1`.

### `Common.Application/Exceptions/`

- `InvalidCommandException : Exception` — carries `List<string> Errors`; thrown by validation decorators. Sibling of `NotFoundException`. Will map to HTTP 422 in Sprint 4 exception middleware.

### `Common.Application`

- `NotFoundException` (`Common/Application/Exceptions/`) — carries `EntityName` + `Id`. Thrown from handlers when a `GetByIdAsync` or `QuerySingleOrDefaultAsync` returns `null`. Will map to HTTP 404 in Sprint 4.

### `Games.Application` — Configuration

- `Configuration/Commands/` — `ICommand`, `ICommand<TResult>`, `ICommandHandler<>`, `ICommandHandler<,>`, `CommandBase`, `CommandBase<TResult>`. (Per-module by design — see `SKILLS.md` rule #10.)
- `Configuration/Queries/` — `IQuery<TResult>`, `IQueryHandler<,>`, `QueryBase<TResult>` (carries `Id` for pipeline correlation).
- `Contracts/` — module-facing types referenced by command/query bases.

### `Games.Application` — Links

- Commands (5):
  - `CreateLinkCommand` / `CreateLinkCommandHandler` — inserts a new Link with category check.
  - `AddOutgoingLinkCommand` / `AddOutgoingLinkCommandHandler`.
  - `RemoveOutgoingLinkCommand` / `RemoveOutgoingLinkCommandHandler`.
  - `ActivateLinkCommand` / `ActivateLinkCommandHandler`.
  - `DeactivateLinkCommand` / `DeactivateLinkCommandHandler`.
- Queries (3):
  - `GetLinkDetailsQuery` → `LinkDetailsDto(Id, CategoryId, Value, Description, IsActive)`.
  - `GetLinksByCategoryQuery` → `List<LinkListItemDto(Id, Value, IsActive)>`.
  - `GetLinkOutgoingLinksQuery` → `List<OutgoingLinkDto(Id, Value, IsActive)>` via JOIN through the `LinkOutgoingLinks` join table.
- All handlers `internal`; all DTOs positional `record`s; SQL via raw string literals against `[Games].[v_Links]`.

### `Games.Application` — Games

- Commands (7):
  - `CreateGameCommand : CommandBase<Guid>` / `CreateGameCommandHandler` — verifies Category, fetches active Link ids, constructs `Puzzle.Create(...)` then `Game.Create(...)`, persists via `IGameRepository.AddAsync`. Handler injects `ICategoryRepository`, `ILinkRepository`, `IGameRepository`, `IPathFinderService`, `IGameConfigurationService`, and `Random`.
  - `StartGameCommand : CommandBase` / `StartGameCommandHandler` — loads Game (404 if missing) and calls `game.Start()`.
  - `MakeStepCommand : CommandBase` / `MakeStepCommandHandler` — loads Game and calls `game.MakeStep(LinkId, ILinkNeighborResolver, IScoreCalculator)`. The two domain services are injected and forwarded as method parameters (per SKILLS.md rule #5).
  - `UseHintCommand : CommandBase<HintResultDto>` / `UseHintCommandHandler` — loads Game, calls `game.UseHint()` (now returns `HintResult`), projects to `HintResultDto(HintType, Guid)`.
  - `UndoCommand : CommandBase` / `UndoCommandHandler` — loads Game and calls `game.Undo()`.
  - `ResetCommand : CommandBase` / `ResetCommandHandler` — loads Game and calls `game.ResetToStart()`.
  - `AbandonGameCommand : CommandBase` / `AbandonGameCommandHandler` — loads Game and calls `game.Abandon()`.
- Queries (1):
  - `GetGameByIdQuery` → `GameDetailsDto` — `QueryMultipleAsync` against `[Games].[v_Games]` JOIN `[Games].[v_Links]` (3× alias for Start/Target/Current word denormalization) + `[Games].[v_GameHistory]` JOIN `[Games].[v_Links]` for the step list. DTO is positional record with one `init` `History` slot for the nested collection — handler does `dto with { History = history }`.
- DTOs:
  - `HintResultDto(HintType Type, Guid RecommendedLinkId)`.
  - `GameDetailsDto` — flat fields (Id, PlayerId, CategoryId, Difficulty, Start/Target/Current Link+Word, State, Score?, MaxSteps, StepsTaken, Hints/Undos/Resets Total+Used) + `IReadOnlyList<GameHistoryStepDto> History`.
  - `GameHistoryStepDto(int StepNumber, Guid LinkId, string LinkValue)`.

### `Games.Application` — Categories

- Commands (2):
  - `CreateCategoryCommand : CommandBase<Guid>` / `CreateCategoryCommandHandler`.
  - `EditCategoryCommand : CommandBase` / `EditCategoryCommandHandler` — calls `category.EditGeneralInfo(...)`.
- Queries (2):
  - `GetCategoryDetailsQuery` → `CategoryDetailsDto(Id, Name, Description, LinkCount)` — `LinkCount` via correlated subquery.
  - `GetCategoriesQuery` → `List<CategoryListItemDto(Id, Name)>` ordered by `Name`.

### Domain additions (during Sprint 2)

- `Link.Activate()` / `Link.Deactivate()` methods.
- Rules: `LinkMustBeInactiveToActivateRule`, `LinkMustBeActiveToDeactivateRule`.
- Events: `LinkActivatedDomainEvent`, `LinkDeactivatedDomainEvent`.
- (Consequence: domain event count rose from 15 to 17; rule count from 16 to 18.)
- `ILinkRepository.GetActiveIdsByCategoryAsync(CategoryId, ...)` — added for `CreateGameCommand` puzzle generation; the existing `GetIdsByCategoryAsync` returns all link ids regardless of `IsActive`, so this sibling method makes the active-only intent explicit at the call site.
- `Game.UseHint()` return type changed from `void` to `HintResult` — handler needs the value for `HintResultDto` projection. The `_puzzle.RequestHint(...)` value was already produced inside the method and passed to `HintUsedDomainEvent`; we just stop discarding it. Event payload unchanged.

---

## Sprint 1 — Domain Hardening (closed 2026-05-02)

### BuildingBlocks (`Common/Domain`)

- `Entity` (domain event collection + `CheckRule(...)`).
- `Entity<TId>` (Id-based equality + operator overloads).
- `ValueObject` (reflection-cached equality with `IgnoreMemberAttribute` opt-out).
- `IBusinessRule`, `BusinessRuleValidationException`.
- `IDomainEvent : INotification`, `DomainEvent` base (`Id` + `OccurredOn`).
- `IAggregateRoot` marker, `IRepository<T> where T : IAggregateRoot`, `IUnitOfWork`.
- `TypedIdValueBase` rejecting `Guid.Empty`.

### Games module — Domain

- **3 aggregates** with private parameterless ctor + private primary ctor + `internal static Create` factory pattern.
  - `Category` — name (≤ 100, non-empty) + description (≤ 500). 3 invariant rules. 2 events.
  - `Link` — value, description, `_categoryId`, `_outgoingLinks`, `_isActive`. Content immutable. Self-loop, duplicate, and missing-edge rules on outgoing topology. 3 events.
  - `Game` — full state machine (`Initial → InProgress → LastStepWarning → Completed/Failed/Abandoned`); commands `Start`, `MakeStep`, `UseHint`, `Undo`, `ResetToStart`, `Abandon`; private `EvaluatePostStepTransitions` for post-step state evaluation; private `Complete(IScoreCalculator)` and `Fail()`. 8 game-level rules (allowance rules nested in VOs). 10 events.
- **Puzzle** moved from separate aggregate → `sealed class Puzzle : ValueObject` inside Game. Domain services (`IPathFinderService`, `IGameConfigurationService`, `Random`) received as method parameters of `Puzzle.Create` — never stored. `ChooseTargetLink` private helper isolates the one `!` null-forgiving operator inside `Create`.
- **Allowance VO family** — `HintAllowance`, `UndoAllowance`, `ResetAllowance` each with `Of(int total)` factory + `Consume()` returning new instance after `CheckRule(*MustHaveRemainingRule)`. Three separate VOs (not generic), so types stay semantic.
- **`StepBudget` VO** — replaces the loose `_maxSteps` int + `history.Count` arithmetic that was previously scattered across Game.cs. Encapsulates `Step()`, `UndoStep()`, `Reset()`, and the `IsExhausted` / `IsAtLastWarning` / `IsBelowLastWarning` predicates.
- **Score VO + `IScoreCalculator`** — `Score` is a pure VO (`Points` + `Of(int)`); formula extracted to `IScoreCalculator`. Default `StandardScoreCalculator` lives in Domain (pure logic). Difficulty multiplier: Easy=1.00, Medium=1.15, Hard=1.25.
- **Repository contracts in Domain** — `ICategoryRepository`, `ILinkRepository`, `IGameRepository`. Implementations deferred to Sprint 3.

### Bug fixes / cleanup during Sprint 1

- `GameMustBeNotStartedRule` — inverted-logic bug fixed.
- `Score.CalculateScore` — `targetDepth = stepsTaken` bug fixed (now `_puzzle.Depth`).
- Old `_remaining{Hints,Undos,Resets}` + `_used` int pairs removed; replaced by allowance VOs.
- Old `GameMustHaveRemaining*Rule` files deleted (rules moved into the VOs).
- Old `Categories/Rules/` and `Puzzles/` folders cleaned up.
- `Link._outgoinglinks` renamed to `_outgoingLinks` (lowercase 'l' typo).

---

## See Also

- `activeContext.md` — what's currently in flight.
- `ROADMAP.md` — what's planned next.
