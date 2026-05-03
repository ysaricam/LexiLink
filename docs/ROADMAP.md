# ROADMAP.md

The forward-looking sprint plan. *What's already shipped* lives in `progress.md`; *what's happening right now* lives in `activeContext.md`.

---

## Sprint 1 — Domain Hardening ✅ closed 2026-05-02

Goal: bring the Games module's Domain layer to production-ready quality.

Outcome: 3 aggregates (Category, Link, Game) with explicit state machines, 18 business rules, 17 domain events, immutable allowance VOs, four domain service interfaces (one with a `StandardScoreCalculator` default impl in Domain), zero `InvalidOperationException` calls in domain code.

Detailed delivery in `progress.md`.

---

## Sprint 2 — Application Layer ⏳ in progress

Goal: make the Domain operable. Wire MediatR, write commands/queries, hook cross-cutting decorators (Kamil-style, Autofac-registered), fill in the read side with Dapper.

### Common.Application

- [x] `NotFoundException` with `EntityName` + `Id` properties.
- [x] `InvalidCommandException` with `List<string> Errors` (thrown by `ValidationCommandHandlerDecorator`).
- [x] `ISqlConnectionFactory` interface (Dapper read side).
- [x] `IUnitOfWork` placement resolved in Sprint 3 — moved Common.Domain → Common.Infrastructure (Kamil-faithful).

### Common.Infrastructure — Domain event dispatch

- [x] `DomainEventsAccessor` — pulls domain events from EF `ChangeTracker.Entries<Entity>()`.
- [x] `DomainEventsDispatcher` — resolves `IDomainEventNotification<>` per event type, publishes via MediatR, serializes notifications into `OutboxMessage`s.
- [x] `DomainEventsDispatcherNotificationHandlerDecorator<T>` — runs `DispatchEventsAsync` after each notification handler so cascade events also dispatch.
- [x] `DomainNotificationsMapper` + `BiDictionary<string, Type>` — bidirectional map for outbox `Type` ↔ runtime type.
- [x] `Serialization/AllPropertiesContractResolver` — Newtonsoft contract resolver that includes private/internal members (so notification payloads serialize fully).

### Modules/Games/Infrastructure/Configuration/Processing/ — Command-handler decorators (Kamil-style, per-module)

Cross-cutting concerns are `ICommandHandler<T>` / `ICommandHandler<T, TResult>` decorators, **not** MediatR `IPipelineBehavior<,>` and **not** in Common. Rationale: `INotificationHandler<T>` (used for in-process domain event dispatch) has no pipeline equivalent, so the notification side must be decorator-based — keeping commands on the same pattern means one mental model. The decorator's generic constraint (`where T : ICommand`) targets Games' own per-module `ICommand`, which forces the decorator to live per-module — exactly Kamil's `Modules/{X}/Infrastructure/Configuration/Processing/` layout.

For each concern, two generics are needed: one for `ICommand` (void return) and one for `ICommand<TResult>` (result-returning commands like `CreateGameCommand : CommandBase<Guid>`, `UseHintCommand : CommandBase<HintResultDto>`).

- [x] `UnitOfWorkCommandHandlerDecorator<T>` — `ICommand` (void) variant; calls `IUnitOfWork.CommitAsync` after the inner handler.
- [x] `UnitOfWorkCommandHandlerWithResultDecorator<T, TResult>` — `ICommand<TResult>` variant.
- [x] `LoggingCommandHandlerDecorator<T>` + `LoggingCommandHandlerWithResultDecorator<T, TResult>` — Serilog-based, Kamil-faithful (LogContext.Push + CommandLogEnricher with `Context = "Command:{Id}"`, try/catch around inner handler). **Deferred:** `RequestLogEnricher` + `IExecutionContextAccessor` (needs Sprint 4 HTTP host) and `IRecurringCommand` short-circuit (needs InternalCommands infra, Beyond Sprint 5). Slot left open in the structure.
- [x] `ValidationCommandHandlerDecorator<T>` + `ValidationCommandHandlerWithResultDecorator<T, TResult>` — FluentValidation, Kamil-faithful: collects `IList<IValidator<T>>`, runs `.Validate(command)` per validator, throws `InvalidCommandException` (in `Common.Application/Exceptions/`) carrying `List<string> Errors` if any rule fails.
- [ ] Autofac registration: `RegisterGenericDecorator(...)` with explicit ordering (logging → validation → unit-of-work → inner handler).
- [ ] Per-aggregate FluentValidation `AbstractValidator<T>` classes — written alongside each command (per Kamil's per-module-per-command pattern).

### Games.Application — Configuration ✅ done

- [x] `Configuration/Commands/`: `ICommand`, `ICommand<TResult>`, `ICommandHandler<>`, `ICommandHandler<,>`, `CommandBase`, `CommandBase<TResult>`. Per-module — not in Common.
- [x] `Configuration/Queries/`: `IQuery<TResult>`, `IQueryHandler<,>`, `QueryBase<TResult>` (carries `Id` for log correlation).

### Games.Application — Links ✅ done

- [x] Commands: `CreateLink`, `AddOutgoingLink`, `RemoveOutgoingLink`, `ActivateLink`, `DeactivateLink`.
- [x] Queries: `GetLinkDetails` (with `LinkDetailsDto`), `GetLinksByCategory` (with `LinkListItemDto`), `GetLinkOutgoingLinks` (with `OutgoingLinkDto`).

### Games.Application — Categories (Create/Edit/List/Details done)

- [x] Commands: `CreateCategory`, `EditCategory`.
- [x] Queries: `GetCategoryDetails` (with `CategoryDetailsDto` including `LinkCount`), `GetCategories` (with `CategoryListItemDto`).
- [ ] Decide: `Activate`/`Deactivate` on Category? (Currently no — Category is owner of Links; deactivating would cascade. Open question.)

### Games.Application — Games ✅ done

Resolved decisions:
- `Game.UseHint()` changed from `void` → `HintResult`; handler projects to `HintResultDto`.
- `IGameConfigurationService` left as 5 separate `Resolve*` calls (no consolidation needed — `ResolveMaxSteps` depends on `puzzle.Depth` which isn't known until after `Puzzle.Create`).
- `ILinkRepository.GetActiveIdsByCategoryAsync` added for puzzle generation.

Commands (7):
- [x] `CreateGameCommand` (returns `Guid`)
- [x] `StartGameCommand`
- [x] `MakeStepCommand`
- [x] `UseHintCommand` (returns `HintResultDto`)
- [x] `UndoCommand`
- [x] `ResetCommand`
- [x] `AbandonGameCommand`

Queries (1):
- [x] `GetGameByIdQuery` — `QueryMultipleAsync` against `[Games].[v_Games]` (JOIN `v_Links` ×3 for Start/Target/Current word denormalization) + `[Games].[v_GameHistory]` (JOIN `v_Links`) → `GameDetailsDto`.

DTOs:
- [x] `GameDetailsDto` — flat fields + `IReadOnlyList<GameHistoryStepDto> History` (positional record + one `init` slot for the nested collection; handler does `dto with { History = history }`).
- [x] `HintResultDto(HintType Type, Guid RecommendedLinkId)`.
- [x] `GameHistoryStepDto(int StepNumber, Guid LinkId, string LinkValue)`.

---

## Sprint 3 — Games.Infrastructure ⏳ in progress

Goal: persist the write model with EF Core (PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`), implement the domain services that need I/O. Pattern: birebir Kamil-faithful — Postgres is the only deviation.

### Common.Infrastructure (Kamil's BuildingBlocks/Infrastructure equivalents)

- [x] `IUnitOfWork` moved from `Common.Domain` → `Common.Infrastructure` (Kamil-faithful). Decorator imports updated.
- [x] `UnitOfWork : IUnitOfWork` — injects `DbContext` + `IDomainEventsDispatcher`; `CommitAsync` calls `DispatchEventsAsync()` then `_context.SaveChangesAsync(ct)`. Birebir Kamil.
- [x] `TypedIdValueConverter<TTypedIdValue>` — `TypedIdValueBase` ↔ `Guid` value converter (Andrew Lock pattern).
- [x] `StronglyTypedIdValueConverterSelector` — auto-applies `TypedIdValueConverter` to any property whose type derives from `TypedIdValueBase`.
- [ ] `internalCommandId` parameter on `IUnitOfWork.CommitAsync` — deferred until InternalCommands infrastructure (Beyond Sprint 5).

### DbContext

- [x] `GamesContext : DbContext` at `Modules/Games/Infrastructure/`, with `DbSet<Category>`, `DbSet<Link>`, `DbSet<Game>`. Ctor `(DbContextOptions, ILoggerFactory)`. `OnModelCreating` → `ApplyConfigurationsFromAssembly`.
- [x] Schema: `games` (Postgres lowercase convention; same name as the module).
- [ ] Composition root wires up `UseNpgsql(...)`, `ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>()`, `UseSnakeCaseNamingConvention()` decision (Sprint 4).

### Fluent API configuration

- [x] `CategoryEntityTypeConfiguration` — `_name`, `_description` backing field access; table `Categories` in schema `games`.
- [ ] `LinkEntityTypeConfiguration` — `_outgoingLinks` collection mapping (JSON column or owned collection — decide); `_isActive` backing field.
- [ ] `GameEntityTypeConfiguration` — owned-type mapping for `Puzzle`, `HintAllowance`, `UndoAllowance`, `ResetAllowance`, `Score`, `StepBudget`; `_history` collection mapping.
- [ ] `UsePropertyAccessMode(PropertyAccessMode.Field)` on every aggregate — resolves all CS8618 warnings.

### Repositories

- [x] `CategoryRepository : ICategoryRepository` — `internal`, injects `GamesContext`, methods `GetByIdAsync`, `AddAsync`. No `Commit()` on the repository — UoW decorator handles `SaveChanges`.
- [ ] `LinkRepository : ILinkRepository`
- [ ] `GameRepository : IGameRepository`

### Domain service implementations

- [ ] `PathFinderService : IPathFinderService` — BFS for `FindOptimalPath`; weighted variant for `FindTarget` at depth.
- [ ] `LinkNeighborResolver : ILinkNeighborResolver` — wraps `ILinkRepository`.
- [ ] `GameConfigurationService : IGameConfigurationService` — config-driven (Easy/Medium/Hard → depth range, step budget, allowance counts).

### Read-side views

- [ ] `[Games].[v_Categories]`, `[Games].[v_Links]`, `[Games].[v_Games]`, `[Games].[v_GameHistory]`.
- [ ] `LinkOutgoingLinks` join table view for `GetLinkOutgoingLinks`.

### Migrations

- [ ] Initial migration: tables, views, indices.
- [ ] CS8618 warning count: 0.

---

## Sprint 4 — API Host

Goal: boot the system end-to-end with HTTP endpoints.

- [ ] `LexiLink.API` ASP.NET Core project.
- [ ] `IModule` / `ModuleStartup` pattern — each module registers its own DI, MediatR scan, FluentValidation.
- [ ] `GamesModule` startup wiring all of Games.Application + Games.Infrastructure.
- [ ] Endpoints (Minimal API or Controllers — TBD): Categories, Links, Games CRUDs and game actions.
- [ ] Exception middleware:
  - `BusinessRuleValidationException` → 400 Bad Request with `BrokenRule` + `Details`.
  - `NotFoundException` → 404 Not Found with `EntityName` + `Id`.
  - `ValidationException` (FluentValidation) → 422 Unprocessable Entity.
- [ ] OpenAPI / Swagger.
- [ ] `appsettings.json` + environment-specific configs.
- [ ] First end-to-end smoke test: create a Category → add Links → wire outgoing topology → create Game → make a step → complete.

---

## Sprint 5 — Tests

Goal: lock down the contract surface with three flavors of test.

### Domain unit tests (`Games.Tests`)

- [ ] `BusinessRuleTestExtensions.AssertBrokenRule<TRule>(Action)` helper.
- [ ] One test per aggregate state transition (Game state machine; Link Activate/Deactivate; Category EditGeneralInfo).
- [ ] One test per business rule (positive + negative).
- [ ] Allowance VO tests: `Of(0).Consume()` throws; `Consume()` returns new instance with `Used+1`.
- [ ] `StandardScoreCalculator`: formula correctness across all three difficulty multipliers.

### Architecture tests (NetArchTest or ArchUnitNET)

- [ ] Aggregates implement `IAggregateRoot`.
- [ ] Domain layer has no `Application` / `Infrastructure` references.
- [ ] No `public` constructor on `Entity<TId>` subclasses.
- [ ] No `InvalidOperationException` thrown from Domain (except `TypedIdValueBase`).
- [ ] All `*DomainEvent` types end with `DomainEvent`.
- [ ] All `*Rule` types implement `IBusinessRule`.

### Integration tests

- [ ] Command → handler → repository → real (test) DB → DbContext SaveChanges path for one command per aggregate.
- [ ] Domain event handler dispatched after `IUnitOfWork.CommitAsync`.
- [ ] Decorator chain fires in expected order (validation → logging → unit-of-work → handler).

---

## Beyond Sprint 5

- **Player module** — second module; first cross-module integration via Outbox/Inbox.
- **Integration Events / Outbox-Inbox** — required when Player module ships.
- **Read-model projection / CQRS denormalization** — when leaderboard / streak views land.
- **Event sourcing** — explicitly *not* on the path. Game state is durable; events are a notification mechanism, not the source of truth.

---

## See Also

- `progress.md` — what's been delivered and when.
- `activeContext.md` — current sprint focus.
- `SKILLS.md` — the principles every sprint follows.
