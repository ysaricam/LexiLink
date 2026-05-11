# LexiLink — Wikipedia Dataset Extraction Spec

A self-contained specification for generating game-ready Category data for **LexiLink**, a word-graph puzzle game. Hand this document to a developer or AI assistant; the deliverable is one or more JSON files, one per Category, ready to be consumed by a separate C# importer in the LexiLink codebase.

> The spec covers **extraction only**. Database loading is performed by a separate C# importer that calls the existing domain commands (`CreateCategoryCommand`, `CreateLinkCommand`, `AddOutgoingLinkCommand`). The importer is out of scope for this document.

---

## 1. Game context (what we're feeding)

LexiLink is a directed-graph puzzle game. A `Category` contains a set of `Link`s (each `Link` is a word/concept node) plus directed edges between them. A puzzle = a `(start, target)` pair within a Category. Player advances from `start` to `target` by traversing edges. The puzzle generator picks `start` uniformly at random from the Category, then BFS-finds the first node reachable at depth ∈ `[minDepth, maxDepth]`; that node becomes `target`.

Difficulty bands:

| Difficulty | Depth range | Allowances (hint/undo/reset) |
|---|---|---|
| Easy | 3–5 | 3 / 5 / 2 |
| Medium | 5–7 | 2 / 3 / 1 |
| Hard | 7–10 | 1 / 2 / 1 |

The data set must satisfy these **hard domain rules** (enforced at runtime; violations crash the import):

- A Category must contain **≥ 5** Links (`CategoryMustHaveEnoughLinksToStartGameRule`).
- Every outgoing edge `A → B` must have **A and B in the same Category** (`LinkOutgoingMustBeSameCategoryRule`). Cross-Category edges are rejected.
- A Link cannot have an outgoing edge pointing to itself (`LinkCannotPointToItselfRule`).
- Duplicate outgoing edges are rejected (`LinkOutgoingAlreadyExistsRule`).

Additionally, for the puzzle generator to be reliable, the data set should satisfy these **soft properties**:

- For Hard difficulty (depth up to 10) to work from most starts, the largest connected component should reach ≥ 80% of nodes, and graph eccentricity should be ≥ 10 for the majority of nodes.
- Mean out-degree in `[3, 8]`. Below 3, dead-ends dominate. Above 10, BFS hits target at depth 2 too often, collapsing difficulty.
- No Link with out-degree 0 (would deadlock the player).

---

## 2. Algorithm: backward BFS from an anchor, bounded by a Wikipedia category

The core idea: choose a Wikipedia article `T` as the **anchor** (the puzzle's target). Walk **backward** through Wikipedia's link graph (articles that link **to** `T`, then articles that link to *those*, etc.), restricted to a chosen Wikipedia category `C`. The result is a set of nodes guaranteed to have a path to `T` of length ≤ `D_MAX`.

```text
INPUT:
  T          — Wikipedia article title (anchor)
  C          — Wikipedia category title (constraint bound)
  D_MAX      — maximum backward depth (recommended: 10)
  L          — Wikipedia language code (recommended: "tr")
  CAP_LAYER  — per-layer node cap (recommended: 200)

PROCEDURE backwardFanOut(T, C, D_MAX, L, CAP_LAYER):
  members      ← MediaWiki: list=categorymembers&cmtitle=Category:C   (all articles in C)
  membersSet   ← set of canonical titles
  visited      ← {T}
  layer        ← {T}
  depth        ← {T: 0}

  for d in 1..D_MAX:
    next ← {}
    for each x in layer:
      backlinks ← MediaWiki: list=backlinks&bltitle=x        (articles linking to x)
      backlinks ← backlinks ∩ membersSet                     (in-category only)
      backlinks ← backlinks − visited
      next ← next ∪ backlinks
      if |next| ≥ CAP_LAYER: break

    if next is empty: break
    next   ← truncate(next, CAP_LAYER)                       (sample if oversized)
    for each n in next: depth[n] ← d
    visited ← visited ∪ next
    layer   ← next

  return visited, depth
```

**Why this terminates fast:** Wikipedia category constraint bounds the universe. Most well-chosen pairs `(T, C)` produce 100–400 nodes total within `D_MAX = 10`. Without `C`, popular anchors (e.g., "Albert Einstein") explode to 30k+ at depth 1.

**After backward fan-out:** fetch the **forward edges** of every node in `visited`, keep only edges whose target is also in `visited`. This is the game's edge set. (Backward fan-out tells you *which* nodes belong; forward edges tell you *how* the player navigates.)

---

## 3. Pipeline stages

Each stage outputs an intermediate file, allowing inspection and rerun without re-fetching.

| # | Stage | Input | Output | Purpose |
|---|---|---|---|---|
| 1 | **Fetch members** | `(C, L)` | `members.json` | All Wikipedia article titles in category `C`, canonicalized (redirects resolved) |
| 2 | **Backward BFS** | `members.json`, `T`, `D_MAX` | `backset.json` | Map `{title → depthFromAnchor}` for nodes reachable backward from `T` within `C`, depth ≤ `D_MAX` |
| 3 | **Fetch forward edges** | `backset.json` | `edges.json` | All forward Wikipedia links between nodes in `backset` (both endpoints in `backset`) |
| 4 | **Normalize + filter** | `backset.json`, `edges.json` | `clean.json` | Apply node and edge filters (see §4) |
| 5 | **Symmetrize** | `clean.json` | `symmetric.json` | For every directed edge `A → B`, ensure `B → A` exists (configurable; recommended **on**) |
| 6 | **Graph cleanup** | `symmetric.json` | `final.json` | Keep largest weakly-connected component; drop nodes with out-degree 0 |
| 7 | **Quality check** | `final.json` | `report.txt` | Compute QA metrics (see §6); fail if thresholds not met |
| 8 | **Emit Category JSON** | `final.json` + metadata | `category-{slug}.json` | LexiLink-importer-ready file (see §5 for schema) |

---

## 4. Filters

### 4.1 Node filters (drop the article entirely)

Drop a Wikipedia article from `members` / `backset` if **any** of:

- Title length > 30 characters (UI legibility constraint)
- Title is a "List of …" article (heuristic: starts with `List of` / `Liste of` / TR equivalents)
- Title is a year/date article (regex: `^\d{1,4}$`, `^\d{1,2} (January|February|...|Aralık)$`, etc.)
- Page is a **disambiguation** page (MediaWiki `prop=pageprops`, key `disambiguation`)
- Page is a **redirect** (resolve to canonical title; if canonical not in `backset`, drop)
- Title contains characters outside `[\p{L}\p{N} \-'.,()]` (rejects exotic scripts)
- Page namespace ≠ 0 (no Talk:, Help:, File:, etc.)
- Article is a **stub** (heuristic: page byte length < 2000, or carries `Category:Stubs`)
- Title duplicates an existing title within the dataset, case-insensitively after normalization

### 4.2 Edge filters

Drop an edge `A → B` if any of:

- `A == B` (self-loop)
- `A` or `B` not in the final node set (after node filtering)
- Edge is a duplicate of one already accepted (canonicalize ordering as `(A, B)` and dedupe)

### 4.3 Per-node degree filter

After all node and edge filtering, drop nodes whose post-cleanup out-degree is outside `[3, 20]`:

- Out-degree < 3: dead-end risk; the puzzle generator may BFS-fail.
- Out-degree > 20: noisy / hub article (e.g., "Periodic table" links to every element). Hub nodes collapse difficulty.

After dropping high/low-degree nodes, **re-check connectivity** (a hub's removal may shatter the graph). If the largest weakly-connected component drops below 80% of remaining nodes, undo the most recent drops greedily until threshold is met.

---

## 5. Output JSON schema

One file per Category. File name: `category-{slug}.json` where `slug` is e.g. `chemistry-hydrogen`.

```json
{
  "$schema": "lexilink/category/v1",
  "category": {
    "name": "Kimyasal elementler (Hidrojen)",
    "description": "Hidrojen makalesinden geriye doğru genişletilmiş kimyasal elementler grafı."
  },
  "anchor": {
    "value": "Hidrojen",
    "wikipediaUrl": "https://tr.wikipedia.org/wiki/Hidrojen"
  },
  "links": [
    {
      "value": "Hidrojen",
      "description": "Atom numarası 1 olan, evrendeki en hafif element.",
      "wikipediaUrl": "https://tr.wikipedia.org/wiki/Hidrojen",
      "depthFromAnchor": 0
    },
    {
      "value": "Helyum",
      "description": "Atom numarası 2 olan asal gaz.",
      "wikipediaUrl": "https://tr.wikipedia.org/wiki/Helyum",
      "depthFromAnchor": 1
    }
  ],
  "edges": [
    { "from": "Hidrojen", "to": "Helyum" },
    { "from": "Helyum", "to": "Hidrojen" }
  ],
  "metadata": {
    "wikipediaLanguage": "tr",
    "wikipediaCategory": "Kimyasal_elementler",
    "depthMax": 10,
    "symmetrized": true,
    "nodeCount": 142,
    "edgeCount": 612,
    "largestComponentRatio": 0.96,
    "averageOutDegree": 4.31,
    "diameter": 9,
    "generatedAt": "2026-05-09T12:00:00Z",
    "generatorVersion": "1.0.0"
  }
}
```

### Field rules

- `links[].value` — must be **unique** within the file (case-sensitive). The importer uses it as the human-readable display word AND as the lookup key for resolving `edges`. Length **≤ 30** characters (matches `CategoryNameMustNotExceedMaxLengthRule`-equivalent surface validation in `CreateLinkCommandValidator`).
- `links[].description` — short Wikipedia lead summary (first sentence preferred), **≤ 500** characters. Empty string is allowed.
- `edges[].from` and `edges[].to` — must reference an existing `links[].value`. Importer rejects the file if any reference is dangling.
- `anchor.value` — must equal exactly one `links[].value` (the node at `depthFromAnchor: 0`).
- `metadata.symmetrized` — if `true`, every `(A, B)` edge must have a `(B, A)` counterpart in the array.

---

## 6. Quality assurance

Before emitting `category-{slug}.json`, the pipeline must verify and **fail loudly** if any threshold is missed:

| Metric | Threshold | Why |
|---|---|---|
| Node count | ≥ 40 | Below this, puzzle generator's random start picks repeat too often. |
| Node count | ≤ 400 | Above this, page load / Dapper queries get heavy. |
| Largest weakly-connected component | ≥ 80% of nodes | Disconnected regions break random-start puzzles. |
| Mean out-degree | ∈ `[3.0, 8.0]` | Outside this, difficulty bands collapse or fail. |
| Diameter (longest shortest path) | ≥ 10 | Hard difficulty (depth 7–10) needs reachable depth-10 pairs. |
| Sampled puzzle success rate | ≥ 80% | Sample 100 random `(start, depth=10)` pairs, run BFS; ≥ 80 must find a target. Repeat for depth=5 and depth=3. |
| Has node at `depthFromAnchor` for each `d ∈ {0..D_MAX}` | true (with at most 1 missing layer) | Confirms the anchor-distance distribution covers the difficulty range. |

Emit the QA metrics into `metadata.*` so the importer / future debug sessions can read them.

---

## 7. Configuration parameters (set per run)

| Parameter | Recommended default | Rationale |
|---|---|---|
| `T` (anchor article) | **Hand-pick** (10–15 anchors curated upfront) | Heuristic picks (e.g., highest pageviews) often select meta-articles like "Wikipedia" or "Türkiye" that don't make good puzzle anchors. Manual curation costs ~30 min one-time. |
| `C` (Wikipedia category) | The article's most-specific subject category | Keeps the BackSet thematically coherent. |
| `L` (language) | `tr` | Turkish Wikipedia is sparser than English, so backward fan-out converges faster — exactly what we want. Audience reads Turkish. Switch to `en` per-Category if the topic has limited TR coverage. |
| `D_MAX` | **10** | Matches Hard difficulty's max depth. |
| `CAP_LAYER` | 200 | Caps per-layer expansion when a popular article appears in `backset`. Layers beyond 200 nodes get random-sampled. |
| Symmetrize | **true** | The game UX feels bidirectional ("cat ↔ mat" is natural). Wikipedia link asymmetry produces unfair puzzles otherwise. Edge count doubles but stays small. |
| Stub byte threshold | 2000 | Drops near-empty articles. |
| Out-degree band | `[3, 20]` | See §4.3. |

---

## 8. Tech stack suggestion (non-prescriptive)

- **Python 3.11+** — for fetch, filtering, graph analysis.
- **`requests`** — direct MediaWiki API calls. The `wikipedia` PyPI package is ergonomic but limits concurrency; raw API is fine.
- **`networkx`** — graph operations (connected components, BFS, diameter, eccentricity). Used for both filtering and QA.
- **MediaWiki API endpoints used:**
  - `action=query&list=categorymembers&cmtitle=Category:{C}&cmlimit=500&cmnamespace=0` — articles in category
  - `action=query&list=backlinks&bltitle={title}&bllimit=500&blnamespace=0` — articles linking to a title
  - `action=query&prop=links&titles={title}&pllimit=500&plnamespace=0` — outgoing links from an article
  - `action=query&prop=extracts&titles={title}&exintro=1&explaintext=1` — first-paragraph summary for `description`
  - `action=query&prop=pageprops&titles={title}&ppprop=disambiguation` — disambiguation flag
  - `action=query&titles={title}&redirects=1` — resolve redirect to canonical
- **Rate limit:** 200 requests/min anonymous. Add a `time.sleep(0.3)` between calls or use `aiohttp` with semaphore. Cache responses on disk by URL hash.

---

## 9. Pitfalls / gotchas

- **Redirects:** `Hidrojen molekülü` may redirect to `Hidrojen`. Always resolve before adding to any set.
- **Disambiguation pages:** `Mercury` may be the planet, the element, or the messenger. Always drop unless explicitly the topic.
- **Inter-language links** (the "languages" sidebar): NOT internal links — do not follow.
- **Templates and infoboxes** add many backlinks that aren't semantic ("article uses {Periodic table}" produces a backlink from every element). Heuristic to detect: if a single page contributes > 30 backlinks at depth 1, it's probably a template, downweight or skip.
- **Self-loops via redirects:** `A → redirect-of-A`; resolve redirects before edge collection.
- **Wikipedia search vs. exact title:** Always use exact title API calls; search is fuzzy.
- **Pagination cursors** (`continue` parameter) — backlinks of popular articles need pagination; don't truncate the first 500.
- **Title casing:** Wikipedia API returns titles with the original casing. Don't lowercase before storage; the importer is case-sensitive on `value`.
- **HTTP retries:** Wikipedia returns 503 under load. Implement exponential backoff (1s, 2s, 4s, 8s) before giving up.

---

## 10. Acceptance criteria

A run is successful and ready to import if **all** are true:

1. ✅ At least one `category-{slug}.json` file produced.
2. ✅ The file validates against the schema in §5 (machine-checkable: every `edges[].from/to` resolves to a `links[].value`; `anchor.value` equals one `link.value`).
3. ✅ All thresholds in §6 are met (no warnings in `report.txt`).
4. ✅ All character/length filters in §4 are applied (no `links[].value` longer than 30 chars; no list/year/disambiguation pages).
5. ✅ `metadata.symmetrized=true` ⇒ the edges array is symmetric (every `(A, B)` has a `(B, A)`).
6. ✅ The pipeline can be **rerun deterministically** for the same `(T, C, L, D_MAX, seed)` and produces the same output (use a fixed random seed for the layer-sampling step).

---

## 11. Suggested first targets to extract

To validate the pipeline end-to-end, start with two contrasting categories:

| Slug | Anchor (`T`) | Wikipedia category (`C`) | Why |
|---|---|---|---|
| `chemistry-hydrogen` | Hidrojen | Kimyasal_elementler | Periodic table is dense, ~118 nodes globally — bounded and well-defined. Great calibration target. |
| `mythology-zeus` | Zeus | Yunan_mitolojisi | Narrative cross-references give organic depth-7 paths. Tests the pipeline against narrative-heavy data. |

If both pass §10 acceptance, the pipeline is production-ready. Then expand to ~10 more anchors per the manual-curation strategy.

---

## 12. Out of scope

- Database loading: a separate C# importer (`tools/LexiLink.DatasetImporter/`) reads these JSON files and creates Category/Link/OutgoingLink via the existing domain commands. It validates against domain rules a second time and emits a per-file report.
- Localization: text in `description` is single-language per file (matches `wikipediaLanguage`). The importer doesn't translate.
- Updates / diffs: the pipeline produces full snapshots. Incremental updates / migration of existing Categories are not in v1.
