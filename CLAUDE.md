# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project at a Glance

LexiLink is a **.NET 10 Modular Monolith** following **Domain-Driven Design** in the style of Kamil Grzybek's [modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd) reference. It's a word-graph puzzle game: players step from a start word to a target word through a directed graph of category-bound `Link`s.

**Sprints 1–7 are closed.** Games module is mature end-to-end (Domain, Application, Infrastructure, API, DbUp schema, unit + integration tests). Players module shipped as the second module on 2026-05-11 and validates the modular monolith pattern with its own schema, outbox, decorators, API endpoints, and tests. Next likely roadmap step: Sprint 8 Stats module.

## Documentation Map

All project docs live under `docs/`. Read the relevant one before non-trivial changes — they're short and targeted:

- **`docs/SKILLS.md`** — Project rules. Kamil-style MM + DDD principles enforced in this codebase, with DO/DON'T snippets. **Read this before any domain or application change.**
- **`docs/GLOSSARY.md`** — Ubiquitous language. Every aggregate, VO, rule, event, and service explained.
- **`docs/CONVENTIONS.md`** — Naming, file layout, visibility, DTO style, SQL conventions.
- **`docs/ROADMAP.md`** — Sprint plan with detailed checklists.
- **`docs/activeContext.md`** — What's happening *right now* + recent surprising design choices.
- **`docs/progress.md`** — What's been delivered and when.

## Commands

```bash
# Build whole solution
dotnet build LexiLink.sln

# Build a single project (faster feedback when iterating on Domain)
dotnet build src/Modules/Games/Domain/LexiLink.Modules.Games.Domain.csproj

# Run all tests (NUnit)
dotnet test

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
  Domain/ Application/ Infrastructure/ Tests/ IntegrationTests/
src/Modules/Players/
  Domain/ Application/ Infrastructure/ Tests/ IntegrationTests/
src/API/LexiLink.API/                  # Minimal API host
src/Database/                          # DbUp migrator + SQL structure files
```

Aggregates: Games module has **Category**, **Link**, **Game**; Players module has **Player**. Cross-aggregate references are by Id only (`TypedIdValueBase`). Full descriptions in `GLOSSARY.md`.

## When Proposing Changes

- **Read the relevant doc first.** `SKILLS.md` for principles, `GLOSSARY.md` for terminology, `activeContext.md` for what's currently in flight and recent design choices that look weird but are intentional.
- **The user pushes back on speculative abstraction.** If you're adding an interface "in case", flag the tradeoff and let the user decide.
- **For exploratory questions** ("should we…?"), answer with a recommendation + tradeoff in 2-3 sentences and **wait** before implementing. Don't preemptively bundle services, split methods into many helpers, or wrap primitives unless the case is concrete.
- **Verify Kamil's rationale before critiquing apparent redundancies.** Several patterns in this codebase look redundant (per-module CQRS contracts, `QueryBase.Id`, etc.) — they're intentional and the payoff often emerges with later infrastructure. `activeContext.md` lists the current set.

## Communication

The user works in Turkish. **Project documentation is in English** (so the doc set stays internally consistent). Code identifiers, comments, rule messages, and chat replies follow the user's lead — Turkish in conversation, English in code and docs.
