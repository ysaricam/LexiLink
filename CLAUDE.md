# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project at a Glance

LexiLink is a **.NET 10 Modular Monolith** following **Domain-Driven Design** in the style of Kamil Grzybek's [modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd) reference. It's a word-graph puzzle game: players step from a start word to a target word through a directed graph of category-bound `Link`s.

**Sprints 1–7 are closed and Stats + Energy + Quests modules have shipped.** Games module is mature end-to-end (Domain, Application, Infrastructure, API, DbUp schema, unit + integration tests). Players module shipped as the second module on 2026-05-11 and validates the modular monolith pattern. Stats module is a read-model/projection module driven by integration events. Energy module shipped on 2026-05-14 as the first business module with synchronous cross-module integration: `Games.Application/IEnergyGuard` is invoked by `StartGameCommandHandler` before `game.Start()`, and Energy listens to `PlayerRegisteredIntegrationEvent` for lazy aggregate initialization. Quests module shipped on 2026-05-15 as the fifth module — daily/play-driven quests with event-driven reward delivery; introduces LexiLink's **first reverse cross-module event dependency** (`Energy.Application` consumes `Quests.IntegrationEvents.QuestClaimedIntegrationEvent` and grants bonus energy via `PlayerEnergy.GrantBonus`, which permits over-max balance).

## Documentation Map

All project docs live under `docs/`. Read the relevant one before non-trivial changes — they're short and targeted:

- **`docs/SKILLS.md`** — Project rules. Kamil-style MM + DDD principles enforced in this codebase, with DO/DON'T snippets. **Read this before any domain or application change.**
- **`docs/GLOSSARY.md`** — Ubiquitous language. Every aggregate, VO, rule, event, and service explained.
- **`docs/CONVENTIONS.md`** — Naming, file layout, visibility, DTO style, SQL conventions.
- **`docs/ROADMAP.md`** — Sprint plan with detailed checklists.
- **`docs/activeContext.md`** — What's happening *right now* + recent surprising design choices.
- **`docs/progress.md`** — What's been delivered and when.
- **`docs/OPERATIONS.md`** — Runtime config, required env vars, health checks, operational endpoints, and migration/run guidance.

## Commands

```bash
# Build whole solution
dotnet build LexiLink.sln

# Build a single project (faster feedback when iterating on Domain)
dotnet build src/Modules/Games/Domain/LexiLink.Modules.Games.Domain.csproj

# Run the project quality gate.
# Integration test projects share a local Postgres database, so this script runs
# DB-free projects first and DB-dependent projects serially. The script also
# passes -m:1 to keep MSBuild from spawning parallel worker nodes.
./scripts/test.sh

# Equivalent one-liner when you need solution-level execution:
dotnet test LexiLink.sln -m:1

# Run tests for a single project
dotnet test src/Modules/Games/Tests/LexiLink.Modules.Games.Tests.csproj

# Run integration tests for a module
dotnet test src/Modules/Players/IntegrationTests/LexiLink.Modules.Players.IntegrationTests.csproj

# Run a single test by fully qualified name
dotnet test --filter "FullyQualifiedName~Game_Should_Transition_To_Completed"
```

`net10.0`, nullable enabled, MediatR 14, NUnit 4, FluentAssertions 6.12, NSubstitute 5.1, Dapper 2.1.72, EF Core 10/Npgsql. API host exists under `src/API/LexiLink.API`; DbUp schema deployment is operator-run via `src/Database/LexiLink.DatabaseMigrator`.

## Project Layout (one-glance)

```
src/Common/                            # BuildingBlocks
  Domain/ Application/ Infrastructure/ Tests/
src/Modules/Games/
  Domain/ Application/ Infrastructure/ IntegrationEvents/ Tests/ IntegrationTests/
src/Modules/Players/
  Domain/ Application/ Infrastructure/ IntegrationEvents/ Tests/ IntegrationTests/
src/Modules/Stats/
  Application/ Infrastructure/ IntegrationTests/   # projection-only, no Domain
src/Modules/Energy/
  Domain/ Application/ Infrastructure/ Tests/ IntegrationTests/
src/Modules/Quests/
  Domain/ Application/ Infrastructure/ IntegrationEvents/ Tests/ IntegrationTests/
src/API/LexiLink.API/                  # Minimal API host
  CrossModule/                         # API-host adapters for sync cross-module gateways
src/Database/                          # DbUp migrator + SQL structure files
```

Aggregates: Games module has **Category**, **Link**, **Game**; Players module has **Player**; Energy module has **PlayerEnergy**; Quests module has **PlayerQuest**. Cross-aggregate references are by Id only (`TypedIdValueBase`). Full descriptions in `GLOSSARY.md`.

## When Proposing Changes

- **Read the relevant doc first.** `SKILLS.md` for principles, `GLOSSARY.md` for terminology, `activeContext.md` for what's currently in flight and recent design choices that look weird but are intentional.
- **The user pushes back on speculative abstraction.** If you're adding an interface "in case", flag the tradeoff and let the user decide.
- **For exploratory questions** ("should we…?"), answer with a recommendation + tradeoff in 2-3 sentences and **wait** before implementing. Don't preemptively bundle services, split methods into many helpers, or wrap primitives unless the case is concrete.
- **Verify Kamil's rationale before critiquing apparent redundancies.** Several patterns in this codebase look redundant (per-module CQRS contracts, `QueryBase.Id`, etc.) — they're intentional and the payoff often emerges with later infrastructure. `activeContext.md` lists the current set.

## Communication

The user works in Turkish. **Project documentation is in English** (so the doc set stays internally consistent). Code identifiers, comments, rule messages, and chat replies follow the user's lead — Turkish in conversation, English in code and docs.
