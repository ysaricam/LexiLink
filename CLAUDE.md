# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project at a Glance

LexiLink is a **.NET 10 Modular Monolith** following **Domain-Driven Design** in the style of Kamil Grzybek's [modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd) reference. It's a word-graph puzzle game: players step from a start word to a target word through a directed graph of category-bound `Link`s.

**Sprint 1 closed 2026-05-02; Sprint 2 (Application layer) is in progress.** Domain layer is mature; Infrastructure / API host / Tests are still empty.

## Documentation Map

All project docs live under `docs/`. Read the relevant one before non-trivial changes — they're short and targeted:

- **`docs/SKILLS.md`** — Project rules. Kamil-style MM + DDD principles enforced in this codebase, with DO/DON'T snippets. **Read this before any domain or application change.**
- **`docs/GLOSSARY.md`** — Ubiquitous language. Every aggregate, VO, rule, event, and service explained.
- **`docs/CONVENTIONS.md`** — Naming, file layout, visibility, DTO style, SQL conventions.
- **`docs/ROADMAP.md`** — Sprint plan (1–5) with detailed checklists.
- **`docs/activeContext.md`** — What's happening *right now* + recent surprising design choices.
- **`docs/progress.md`** — What's been delivered and when.

## Commands

```bash
# Build whole solution
dotnet build LexiLink.sln

# Build a single project (faster feedback when iterating on Domain)
dotnet build src/Modules/Games/Domain/LexiLink.Modules.Games.Domain.csproj

# Run all tests (xUnit)
dotnet test

# Run tests for a single project
dotnet test src/Modules/Games/Tests/LexiLink.Modules.Games.Tests.csproj

# Run a single test by fully qualified name
dotnet test --filter "FullyQualifiedName~Game_Should_Transition_To_Completed"
```

`net10.0`, nullable enabled, MediatR 14, xUnit 2.9, Dapper 2.1.72. There is no API host project yet, so the solution doesn't `dotnet run` end-to-end.

## Project Layout (one-glance)

```
src/Common/                            # BuildingBlocks
  Domain/                              # mature
  Application/                         # NotFoundException only — see ROADMAP Sprint 2
  Infrastructure/  Tests/              # placeholders
src/Modules/Games/
  Domain/                              # mature — 3 aggregates (Category, Link, Game)
  Application/                         # in progress — Categories + Links done; Games next
  Infrastructure/  Tests/              # placeholders
```

Aggregates (Games module): **Category**, **Link**, **Game**. Cross-aggregate references are by Id only (`TypedIdValueBase`). Full descriptions in `GLOSSARY.md`.

## When Proposing Changes

- **Read the relevant doc first.** `SKILLS.md` for principles, `GLOSSARY.md` for terminology, `activeContext.md` for what's currently in flight and recent design choices that look weird but are intentional.
- **The user pushes back on speculative abstraction.** If you're adding an interface "in case", flag the tradeoff and let the user decide.
- **For exploratory questions** ("should we…?"), answer with a recommendation + tradeoff in 2-3 sentences and **wait** before implementing. Don't preemptively bundle services, split methods into many helpers, or wrap primitives unless the case is concrete.
- **Verify Kamil's rationale before critiquing apparent redundancies.** Several patterns in this codebase look redundant (per-module CQRS contracts, `QueryBase.Id`, etc.) — they're intentional and the payoff often emerges with later infrastructure. `activeContext.md` lists the current set.

## Communication

The user works in Turkish. **Project documentation is in English** (so the doc set stays internally consistent). Code identifiers, comments, rule messages, and chat replies follow the user's lead — Turkish in conversation, English in code and docs.
