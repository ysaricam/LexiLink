# CONVENTIONS.md

Code-shape conventions for LexiLink. These are the *how* rules — naming, file layout, visibility, formatting. The *why* rules (DDD invariants, aggregate construction, etc.) live in `SKILLS.md`.

---

## Project Layout

```
LexiLink/
├── LexiLink.sln
├── CLAUDE.md  SKILLS.md  GLOSSARY.md  CONVENTIONS.md  ROADMAP.md
├── activeContext.md  progress.md
└── src/
    ├── Common/
    │   ├── Domain/             ← BuildingBlocks (mature)
    │   ├── Application/        ← shared exceptions only — NotFoundException etc.
    │   ├── Infrastructure/     ← (Sprint 3+)
    │   └── Tests/              ← (Sprint 5)
    └── Modules/
        └── Games/
            ├── Domain/         ← aggregates, VOs, rules, events, services (mature)
            ├── Application/    ← commands, queries, handlers, DTOs (in progress)
            ├── Infrastructure/ ← DbContext, repositories, service impls (Sprint 3)
            └── Tests/          ← (Sprint 5)
```

`Common.Application` carries only what every module legitimately shares — base exceptions like `NotFoundException`, the `ISqlConnectionFactory` interface, the `IUnitOfWork` interface (lifted from Domain if needed). It does **not** carry CQRS contracts (`ICommand`/`IQuery`) — those duplicate per module on purpose. See `SKILLS.md` rule #10.

---

## Feature Folder per Command/Query

Each command or query owns a folder. The folder contains the request, the handler, and (for queries) the DTO. This is Kamil Grzybek's layout, kept verbatim.

```
src/Modules/Games/Application/
├── Categories/
│   ├── CreateCategory/
│   │   ├── CreateCategoryCommand.cs
│   │   └── CreateCategoryCommandHandler.cs
│   ├── GetCategoryDetails/
│   │   ├── CategoryDetailsDto.cs
│   │   ├── GetCategoryDetailsQuery.cs
│   │   └── GetCategoryDetailsQueryHandler.cs
│   └── ...
├── Links/
└── Configuration/
    ├── Commands/   ← ICommand, ICommandHandler, CommandBase
    └── Queries/    ← IQuery, IQueryHandler, QueryBase
```

DTO sharing across queries is **not** done. If `GetLinksByCategoryQuery` and `GetLinkOutgoingLinks` happen to return the same shape, each owns its own DTO file. The duplication keeps each query independently refactor-able.

---

## Naming

| Pattern | Example | Notes |
| --- | --- | --- |
| Typed IDs | `CategoryId`, `LinkId`, `GameId` | Inherit `TypedIdValueBase`. Reject `Guid.Empty`. |
| Domain events | `CategoryCreatedDomainEvent`, `OutgoingLinkAddedDomainEvent` | Past tense. Suffix always `DomainEvent`. |
| Business rules | `GameMustBeInProgressRule`, `LinkCannotPointToItselfRule` | Imperative phrasing. Suffix always `Rule`. |
| Allowance VOs | `HintAllowance` | Suffix `Allowance`. `UndoAllowance` and `ResetAllowance` were removed in Sprint UR1; Undo/Reset persistent balances move to their own modules. |
| Domain services | `IPathFinderService`, `IScoreCalculator`, `ILinkNeighborResolver` | Suffixes: `Service`, `Calculator`, `Resolver`, `Finder`. Lives in `Domain/Services/` if pure, `Infrastructure/Services/` if I/O-bound. |
| Repository | `ICategoryRepository`, `ILinkRepository`, `IGameRepository` | One per aggregate root. Suffix `Repository`. |
| Commands | `CreateCategoryCommand`, `EditCategoryCommand`, `RemoveOutgoingLinkCommand` | Imperative verb + noun. Suffix `Command`. |
| Queries | `GetCategoryDetailsQuery`, `GetLinksByCategoryQuery` | `Get<Subject>` form. Suffix `Query`. |
| Handlers | `CreateCategoryCommandHandler`, `GetCategoryDetailsQueryHandler` | Same name as the request + `Handler`. |
| DTOs | `CategoryDetailsDto`, `CategoryListItemDto`, `LinkOutgoingLinkDto` | Suffix `Dto`. Always a positional `record`. |

---

## Visibility

| Element | Modifier |
| --- | --- |
| Aggregate root class | `public` |
| Aggregate parameterless ctor | `private` |
| Aggregate primary ctor (with invariants) | `private` |
| Aggregate `Create(...)` factory | `internal static` |
| Aggregate state-mutating methods (`Start`, `MakeStep`, `Activate`, `EditGeneralInfo`, ...) | `public` |
| Typed IDs (`CategoryId`, `LinkId`, `GameId`) | `public` |
| Domain events | `public` (consumed by handlers in Application) |
| Business rules | `public` (instantiated from inside aggregates and from tests) |
| Allowance VOs | `public sealed` |
| Domain services (interfaces) | `public` |
| `StandardScoreCalculator` | `public` |
| Application command / query / DTO | `public` |
| Application command / query **handler** | `internal` |
| Pipeline behaviors | `internal` |

The boundary: contracts cross the assembly; implementations don't.

---

## DTO Style

DTOs are positional `record`s, not classes:

```csharp
public record CategoryListItemDto(Guid Id, string Name);

public record LinkDetailsDto(
    Guid Id,
    Guid CategoryId,
    string Value,
    string? Description,
    bool IsActive);
```

Why records: Dapper materializes them via positional constructors without needing settable properties; immutability is the default; equality is value-based; and they read more like data declarations than the equivalent class.

Never `class` with get-only auto-properties — Dapper can't materialize those. Never `class` with `init`-only setters — they work but waste syntax for what records do better.

---

## Null Handling on the Read Side

Repositories and Dapper handlers return nullable references when an entity might not exist. The handler is responsible for converting `null` to `NotFoundException`:

```csharp
var category = await _categoryRepository.GetByIdAsync(new CategoryId(id), ct)
               ?? throw new NotFoundException(nameof(Category), id);

var dto = await connection.QuerySingleOrDefaultAsync<CategoryDetailsDto>(...)
          ?? throw new NotFoundException(nameof(CategoryDetailsDto), query.CategoryId);
```

Use `QuerySingleOrDefaultAsync` for primary-key lookups (data integrity invariant — there should be exactly one or none; two is a bug). Use `QueryAsync` for list queries that legitimately return zero or more rows — empty list is not `NotFoundException`.

---

## Read-Side SQL

```csharp
const string sql = """
    SELECT
        [Category].[Id]   AS [Id],
        [Category].[Name] AS [Name]
    FROM [Games].[v_Categories] AS [Category]
    ORDER BY [Category].[Name]
""";

var rows = await connection.QueryAsync<CategoryListItemDto>(
    new CommandDefinition(sql, cancellationToken: cancellationToken));
return rows.AsList();
```

- Raw string literals (`"""..."""`) — no `@""` for SQL.
- `[Schema].[v_*]` views are the read-side surface — never query tables directly. Views isolate the read model from the write model's evolution.
- Always alias both the table (`AS [Category]`) and the column projection (`AS [Id]`) — the explicit aliases survive view restructuring.
- Always pass `CancellationToken` through `CommandDefinition`.
- Parameter names match the property names on the query: `new { query.CategoryId }`.

---

## Soft Delete

Aggregates that need a "removed" state expose `Activate()` / `Deactivate()` instead of `Delete()`. Required pieces:

1. `_isActive` field on the aggregate (default `true` at creation).
2. `Activate()` and `Deactivate()` methods, both calling `CheckRule(...)`.
3. Two rules: `<Aggregate>MustBeInactiveToActivateRule` and `<Aggregate>MustBeActiveToDeactivateRule`.
4. Two events: `<Aggregate>ActivatedDomainEvent` and `<Aggregate>DeactivatedDomainEvent`.
5. Application commands: `Activate<Aggregate>Command` and `Deactivate<Aggregate>Command`.

`Link` is the canonical example. Repositories never expose `Remove*` or `Delete*`.

---

## C# Features Used

- **Raw string literals** (`"""..."""`) for all SQL.
- **File-scoped namespaces** (`namespace LexiLink.Modules.Games.Application.Categories.GetCategories;`).
- **Positional records** for DTOs.
- **Primary constructors** are accepted on aggregates (the `private MyAggregate(...)` form), but the parameterless EF ctor must still exist as a separate `private MyAggregate() { }`.
- **Nullable reference types** are enabled solution-wide (`<Nullable>enable</Nullable>`). Never `#nullable disable`.
- **`required` keyword** is not used (it's incompatible with EF parameterless ctors and the private-factory pattern).

---

## EF Core CS8618

Parameterless EF Core constructors will warn (CS8618: non-nullable field uninitialized). These warnings are **expected** and resolved in Sprint 3 by configuring `UsePropertyAccessMode(PropertyAccessMode.Field)` on each backing field.

Until then:
- Don't `null!`.
- Don't `#pragma warning disable CS8618`.
- Don't make the field nullable to silence it — that changes the runtime model.

---

## See Also

- `SKILLS.md` — the principles these conventions implement.
- `GLOSSARY.md` — what each named element actually represents.
- `ROADMAP.md` — when CS8618 (and other transitional states) get resolved.
