# CONTENT_AUTHORING.md

Repeatable handoff for authoring and importing **per-language game
content** (Category + Link word graphs). This is a **content-ops task, not
a code change** — a Turkish puzzle graph cannot be machine-translated into a
valid German/French/Spanish one, so each language's graph is authored
independently. The code path (model, importer, filtering, admin UI) is
complete as of Sprint CL (CL1–CL3); this document is what an operator or
content author follows to add a new playable language graph without touching
code.

See also: `OPERATIONS.md > Localization And Locale` (runtime/endpoint view),
`GLOSSARY.md > Category` / `Link` (domain terms), `ROADMAP.md > Sprint CL`
(why content localization is Phase 2).

---

## When to use this

- Authoring a brand-new word graph for a language that has none yet
  (e.g. the first `de-DE` / `fr-FR` / `es-ES` graph).
- Adding another category to an existing language.
- Editing/extending an already-imported category (re-import is idempotent).

Locale codes are **region-qualified** to match `Player.Locale`
(`^[a-z]{2}-[A-Z]{2}$`): `tr-TR`, `en-US`, `de-DE`, `fr-FR`, `es-ES`.

---

## The category JSON schema (`lexilink/category/v1`)

Each category is one JSON file. Two reference files live in `docs/`:
`category-animals-en.json` (`en-US`, fully populated) and
`category-spor.json` (`tr-TR`, the original/no-language form).

```jsonc
{
  "$schema": "lexilink/category/v1",
  "category": {
    "name": "Animals",                 // required, non-empty
    "description": "...",              // ≤ 500 chars
    "language": "en-US"               // optional; omit/empty ⇒ defaults to tr-TR
  },
  "links": [
    {
      "value": "cat",                  // required, non-empty, unique within the file
      "description": "A small ...",   // ≤ 500 chars (may be empty)
      "wikipediaUrl": "",             // advisory authoring metadata — NOT imported (see note)
      "depthFromAnchor": 0             // advisory authoring metadata — NOT imported (see note)
    }
    // ...
  ],
  "edges": [
    { "from": "cat", "to": "bat" }     // a single DIRECTED edge cat → bat
    // ...
  ],
  "metadata": {                         // optional, advisory only
    "source": "manual",
    "symmetrized": true,
    "nodeCount": 12,
    "edgeCount": 30,
    "generatedAt": "2026-06-01T00:00:00Z",
    "generatorVersion": "manual-cl2-1.0.0"
  }
}
```

### Field reference

| Field | Imported? | Notes |
| --- | --- | --- |
| `category.name` | ✅ → `games.Categories.Name` | Required, non-empty. |
| `category.description` | ✅ → `games.Categories.Description` | ≤ 500 chars. |
| `category.language` | ✅ → `games.Categories.Language` | Optional; missing/empty ⇒ `tr-TR`. Must match `^[a-z]{2}-[A-Z]{2}$`. |
| `links[].value` | ✅ → `games.Links.Value` | Required, unique within the file. This is the playable word. |
| `links[].description` | ✅ → `games.Links.Description` | ≤ 500 chars; may be empty. |
| `links[].wikipediaUrl` | ❌ advisory | Accepted by the parser but **not** written by the importer today. Keep `""` unless a future slice consumes it. |
| `links[].depthFromAnchor` | ❌ advisory | Accepted but **not** written. Authoring aid only (e.g. distance from a seed word). `-1` in the Spor file means "unset". |
| `edges[]` | ✅ → `games.LinkOutgoingLinks` | One **directed** edge per entry. |
| `metadata` | ❌ advisory | Free-form authoring provenance; ignored by the importer. |

> **Why `wikipediaUrl`/`depthFromAnchor`/`metadata` exist but aren't
> imported:** they're authoring/provenance aids carried in the file format
> from earlier generation experiments. Leave them as-is; do not rely on
> them landing in the DB.

---

## Graph design rules

A `Game` steps from a start word to a target word by following directed
`Link` edges. The graph you author **is** the playable board, so design it
deliberately:

- **Edges are directed.** `{ "from": "cat", "to": "bat" }` lets a player
  move cat → bat, **not** bat → cat. For normal two-way movement, author
  **both** directions (this is what `metadata.symmetrized: true` advertises).
  The Animals reference file pairs every edge with its reverse.
- **Keep the graph connected.** Games are seeded from start/target pairs; an
  isolated word (no incoming/outgoing edges) can never appear in a path. A
  word with edges only one way can be a dead end.
- **Same-length words make for cleaner puzzles** (the Animals graph is all
  3-letter words, one-letter-change steps) but this is a design convention,
  **not** enforced by the importer — any `value` strings and any edges are
  accepted as long as validation passes.
- **Author in the target language.** Word values, descriptions, and the
  category name are all language-specific content. Do not reuse another
  language's `value`s.

---

## Validation (enforced by the importer)

The importer rejects the file (non-zero exit, nothing written) if any of:

| Rule | Message |
| --- | --- |
| Category name empty | `Category name must not be empty.` |
| Bad language format | `Category language must be in BCP 47 short form (e.g. 'tr-TR', 'en-US').` |
| Duplicate `links[].value` | `Duplicate link value: <value>` |
| Edge `from` not in links | `Edge source is missing from links: <value>` |
| Edge `to` not in links | `Edge target is missing from links: <value>` |
| Duplicate `(from, to)` edge | `Duplicate edge: <from>\t<to>` |

The whole import runs in a single DB transaction — a failure leaves the
database untouched.

---

## Language-aware behavior (how same-name categories coexist)

The importer derives **stable GUIDs** that include the language:

- Category id = `StableGuid("category:{language}:{name}")`
- Link id = `StableGuid("category:{language}:{name}:link:{value}")`

Consequences:

- The **same category name** (e.g. "Animals") can be authored independently
  per locale — `Animals [en-US]` and `Animals [de-DE]` are distinct rows
  with distinct ids and never collide.
- Re-importing the **same file** is **idempotent**: category and links
  upsert by id (`ON CONFLICT DO UPDATE`); the importer deletes and rebuilds
  that category's edges each run, so removing an edge from the JSON and
  re-importing removes it from the DB.
- Changing the `language` of an already-imported file produces a **new**
  category (new id) — it does not move the old one. Use the admin UI to
  re-tag an existing category's language in place (see below).

---

## Step-by-step: author a new language graph

Example: a first German graph mirroring the Animals shape.

1. **Copy a reference file.** Start from `docs/category-animals-en.json`
   into `docs/category-animals-de.json`.
2. **Set the language.** `category.language = "de-DE"`.
3. **Author the content in German.** Replace `category.name`,
   `category.description`, every `links[].value`, and the `links[].description`
   with real German content. Pick words and steps that form a valid,
   connected graph in German — **do not** translate edges mechanically from
   English; the letter/step relationships differ per language.
4. **Author the edges.** For each intended move add `{ "from", "to" }`; add
   the reverse entry too for two-way movement. Keep the graph connected.
5. **(Optional) update `metadata`** node/edge counts for your own records.
6. **Import** (see below).
7. **Verify** (see below).

---

## Import

```bash
dotnet run --project src/Tools/LexiLink.Tools.CategoryImporter/LexiLink.Tools.CategoryImporter.csproj -- \
  "$ConnectionStrings__LexiLinkDb" \
  docs/category-animals-de.json
```

On success the tool prints the resolved language, link/edge counts, and the
stable `CategoryId`. Re-run safely — it's idempotent (see above).

> **iCloud note:** this repo lives under iCloud Drive, which can leave
> `* 2.sql` / `* 2.json` duplicate files in `bin/`. They don't affect the
> importer, but if DbUp later complains about "missing scripts", run
> `find . -name "* 2.sql" -delete`.

---

## Verify

1. **Player view (locale filter):**
   `GET /categories?locale=de-DE` returns only the German categories.
2. **Admin view:** open `/admin/content`, set the language filter to German,
   confirm the category and its link count appear; open detail to check
   name/description/language. (Admin endpoint:
   `GET /admin/content/categories?locale=de-DE`.)
3. **Playability:** start a game in that category and confirm start→target
   paths resolve (a connected graph is required).

---

## Editing existing content without re-import

The admin Content UI (`/admin/content`) can **create/edit a category's
name, description, and language** directly (audited under `Games.Category`).
Use it for small fixes and for re-tagging a category's language in place.
**Link/edge structure changes** still go through the JSON + importer path —
the admin UI does not edit the word graph itself in this phase.

---

## Content-ops checklist (new language graph)

- [ ] File copied from a reference, `category.language` set to the
      region-qualified code (`xx-XX`).
- [ ] Category name + description authored in the target language.
- [ ] All `links[].value` authored in the target language, unique, non-empty.
- [ ] Edges author intended moves; reverse edges added for two-way movement;
      graph is connected (no isolated/dead-end words unless intended).
- [ ] `wikipediaUrl`/`depthFromAnchor` left as advisory (`""` / `-1` / 0…n).
- [ ] Importer run succeeds (prints CategoryId; no validation error).
- [ ] `GET /categories?locale=xx-XX` shows the category.
- [ ] `/admin/content` shows it under the language filter with the right
      link count.
- [ ] A game starts and a path resolves in that category.

---

## Out of scope (current phase)

- **Per-`Link` language.** Links inherit language from their owning
  Category; there is no separate `Link.Language`.
- **Admin word-graph editing.** The admin UI edits category metadata, not
  links/edges. Graph authoring is the JSON + importer flow.
- **Backend message localization.** Rule/validation/error messages remain
  English until Phase 3 (error-code translation).
- **Bulk/automated graph generation.** Authoring is manual in this phase;
  the `metadata.generator*` fields are advisory placeholders only.
