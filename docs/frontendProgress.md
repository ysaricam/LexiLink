# frontendProgress.md

Frontend teslim gecmisi. Yeni isler en uste eklenir; gecmis geriye donuk
yeniden yazilmaz.

---

## Slice D5 — Diamond feature + quest 5-reward UI ✅ closed (2026-05-27, working tree)

Sprint D frontend leg. Added Diamond as the fifth visible inventory,
added the admin Diamond console, and widened quest UI/test fixtures
from 4 rewards to 5. Quality gate after D6/D7 follow-up:
**107/107 Flutter tests pass**. `flutter analyze` still reports only
pre-existing info-level warnings outside the Diamond slice.

### New player feature

- `features/diamond/`: `PlayerDiamond { playerId, balance }` DTO,
  `DiamondRepository.getMe()` (`GET /diamond/me`), `DiamondCubit`,
  and `DiamondBadge` using `Icons.diamond_outlined`.
- `home_screen.dart`: creates/provides `DiamondCubit`, loads it on
  authenticated session, disposes it with the rest of the inventory
  cubits, and renders Diamond in the top-right inventory badge row.

### New admin console

- `features/admin_diamond/`: `PlayerDiamondSnapshot` DTO,
  `AdminDiamondRepository` (`fetchSnapshot`, `setBalance`, `grant`,
  `reset`), `AdminDiamondCubit`, and `AdminDiamondScreen`.
- `/admin/diamond` route added; admin shell gets a Diamond
  NavigationRail/Drawer destination; `AdminDiamondPage` wrapper added.
- D7 regression fix: Diamond number dialogs own their
  `TextEditingController` inside a dialog `StatefulWidget`, preventing
  the runtime error "A TextEditingController was used after being
  disposed" when Set balance / Grant diamonds is submitted.

### Quest 5-reward UI

- `admin_quests/data/quest_definition.dart`: added `diamondReward`.
- `admin_quests/data/admin_quests_repository.dart`: Create/Update
  bodies send `diamondReward`.
- `admin_quests/application/admin_quests_cubit.dart`: Create/Update
  signatures widened to five rewards.
- `admin_quests/presentation/quest_definition_form.dart`: fifth
  number input added for `Elmas` and the client-side
  at-least-one-positive validation spans all five rewards.
- `admin_quests/presentation/admin_quests_screen.dart`: row renders
  a Diamond reward badge when positive.
- `quests/data/player_quest.dart` and `quests_screen.dart`: player
  quest DTO/tile render the fifth reward in the existing `Wrap`.

### Tests

- `admin_diamond/data/admin_diamond_repository_test.dart` covers
  admin endpoint paths and request bodies.
- `admin_diamond/presentation/admin_diamond_screen_test.dart` covers
  the Set balance dialog lifecycle regression.
- Admin shell test covers `/admin/diamond` destination.
- Quest/admin quest fixture tests include `diamondReward`.

### Manual verification

Operator verified D7 golden flows on 2026-05-27: single-reward claim
per reward type, mixed 5-reward claim, Diamond admin Set/Grant/Reset
with audit log assertion, `Diamond:InitialBalance` override, and
Diamond badge persistence across reload/JWT refresh.

## Slice UR7 — Undo + Reset features + quest 4-reward UI ✅ closed (2026-05-26, commit 277fcad)

Sprint UR frontend leg. Two new player features, two new admin
consoles, and a 4-reward quest reshape — all shipped in one bundled
commit. Quality gate at slice close: **103/103 Flutter tests pass**,
`flutter analyze` only pre-existing info-level warnings.

### New player features

- `features/undo/`: `PlayerUndo { playerId, balance }` DTO,
  `UndoRepository.getMe()` (GET /undo/me), `UndoCubit`
  (initial/loading/success/failure), `UndoBadge` (Icons.undo,
  `colorScheme.secondary`).
- `features/reset/`: same shape as undo. `ResetBadge` uses
  `Icons.restart_alt` in `colorScheme.error` to visually
  distinguish from the other three inventory badges.

### New admin consoles

- `features/admin_undo/`: `PlayerUndoSnapshot` DTO,
  `AdminUndoRepository` (fetchSnapshot / setBalance / grant /
  reset), `AdminUndoCubit` (full mirror of `admin_hint` with the
  same notFound state on 404 lookup), `AdminUndoScreen` (lookup
  row + balance card + set / grant / reset buttons). Uses
  `useRootNavigator: false` on every showDialog (codified rule
  from F1–F6).
- `features/admin_reset/`: same shape. The "Reset to zero" admin
  action label is unchanged — the Reset module's domain name
  collides with the admin verb, so the screen text reads
  "Reset reset balance" in the confirmation dialog. Awkward but
  consistent with the per-module audit log naming the operator
  chose during UR planning.

### Routing + shell + placeholder

- `/admin/undo` and `/admin/reset` routes added to
  `app_router.dart` between `/admin/hint` and `/admin/audit`.
- `app_admin_shell.dart`: nav destinations `Undo` (`Icons.undo`)
  and `Reset` (`Icons.restart_alt`) between Hint and Audit.
- `admin_placeholder_page.dart`: `AdminUndoPage` and
  `AdminResetPage` thin StatelessWidget wrappers.

### HomeScreen 4-badge row

- `home_screen.dart`: added `UndoCubit` + `ResetCubit` fields,
  init in `initState`, dispose in `dispose`, BlocProvider
  entries, and `loadUndo()` + `loadReset()` invocations on
  session authentication alongside `loadHint()` and
  `loadEnergy()`.
- Top-right badge stack pivoted from 2 badges
  (HintBadge + EnergyBadge) to **4 badges** in a Row:
  Reset → Undo → Hint → Energy, with 8-px spacers between.

### Quest 4-reward UI

- `admin_quests/data/quest_definition.dart`: added `undoReward` +
  `resetReward` fields with `fromJson` parsing.
- `admin_quests/data/admin_quests_repository.dart`: Create +
  Update bodies now send 4 reward fields.
- `admin_quests/application/admin_quests_cubit.dart`: 4-reward
  Create + Update signatures.
- `admin_quests/presentation/quest_definition_form.dart`: two new
  number inputs (Geri al ödülü ↶ + Sıfırla ödülü ↻) added below
  the existing energy + hint row. Form-level
  `_rewardSumError` now spans all four fields and surfaces a
  message ("En az bir ödül (enerji, ipucu, geri al, sıfırla) > 0
  olmalı.") when all four are zero.
- `admin_quests/presentation/admin_quests_screen.dart`: row
  renders one `_RewardBadge` per positive reward (energy/primary,
  hint/tertiary, undo/secondary, reset/error). The reward-summary
  badges sit between the quest name and the inactive badge.
- `quests/data/player_quest.dart`: added `undoReward` +
  `resetReward` to the DTO.
- `quests/presentation/quests_screen.dart`: player tile reward
  row switched from a fixed `Row(mainAxisSize: min)` to
  `Wrap(spacing: 8)` so all four badges flow on narrow
  screens.

### Test fixtures reshape

- `admin_quests/{cubit,repo,screen}_test.dart` and
  `quests/{cubit,repo}_test.dart`: 5 test files updated to
  include `undoReward` + `resetReward` in the JSON payloads
  (mostly 0 except where the test checks a positive non-energy/
  hint reward delivery — e.g. the repo decode test asserts
  `undoReward: 1` flows through the DTO).

### Decisions / lessons (Sprint UR session, 2026-05-26)

- **Badge colors locked at 4 distinct M3 tokens.** Energy →
  primary, Hint → tertiary, Undo → secondary, Reset → error.
  No theme change needed; all four already in use elsewhere.
- **`Wrap` over `Row` for the player tile reward row.** When
  all 4 rewards are positive on a narrow phone screen, the
  badges need to flow to a second line. `Wrap(spacing: 8)`
  handles this without conditional spacers.
- **"Reset reset balance" double-word is intentional.** The
  Reset module's outer verb (admin action: Reset) collides with
  its inner aggregate (PlayerResetInventory). Renaming the
  action to "Clear" or "Zero" would break the per-module
  Set/Grant/Reset audit log pattern. The frontend text reflects
  that pattern verbatim.

---

## Slice H6 — Hint feature + multi-reward quest UI ✅ closed (2026-05-25, commit 4109a93)

Sprint H frontend leg. Two new features and four reshapes shipped as
a single bundled commit. Quality gate: **103/103 Flutter tests pass**,
`flutter analyze` only pre-existing info-level warnings. All errors
flagged by analyze were in stale test fixtures (old `reward` field)
and were repaired in the same commit.

**New `features/hint/` (player UX):**
- `data/player_hint.dart` — `PlayerHint { playerId, balance }` DTO.
- `data/hint_repository.dart` — `getMe()` → `GET /hint/me`.
- `application/hint_cubit.dart` — initial/loading/success/failure
  states; `loadHint()` invoked from `home_screen.dart` on session
  authentication.
- `presentation/hint_badge.dart` — lightbulb icon + balance number in
  `colorScheme.tertiary`. Sits in a Row next to `EnergyBadge` in the
  HomeScreen header.

**New `features/admin_hint/` (admin console):**
- `data/player_hint_snapshot.dart`, `data/admin_hint_repository.dart`
  (fetchSnapshot / setBalance / grant / reset).
- `application/admin_hint_cubit.dart` — full mirror of
  `admin_energy_cubit` with a notFound state for 404 lookups.
- `presentation/admin_hint_screen.dart` — lookup row + balance card +
  set / grant / reset buttons.
- Route `/admin/hint` wired in `app_router.dart` between energy and
  audit; nav destination + icon (`lightbulb_outline`/`lightbulb`)
  added to `app_admin_shell.dart`; `AdminHintPage` wrapper added to
  `admin_placeholder_page.dart`.

**Quest multi-reward reshape:**
- `admin_quests/data/quest_definition.dart` — `reward` split to
  `energyReward` + `hintReward`.
- `admin_quests/data/admin_quests_repository.dart` — Create / Update
  bodies send both fields.
- `admin_quests/application/admin_quests_cubit.dart` — Create /
  Update signatures take both fields.
- `admin_quests/presentation/quest_definition_form.dart` —
  `_rewardController` replaced with `_energyRewardController` +
  `_hintRewardController` (two side-by-side inputs labeled
  "Enerji ödülü ⚡" and "İpucu ödülü 💡"). Each is `≥ 0`; a
  non-field `_rewardSumError` enforces the at-least-one-positive
  rule client-side and surfaces under the row in red.
- `admin_quests/presentation/admin_quests_screen.dart` — row renders
  one `_RewardBadge` per positive reward (energy in
  `colorScheme.primary`, hint in `colorScheme.tertiary`); inactive
  badge stays.
- `quests/data/player_quest.dart` — `reward` field split to
  `energyReward` + `hintReward`.
- `quests/presentation/quests_screen.dart` — player tile renders
  both badges side-by-side when positive; the 8-px spacer appears
  only between two positive rewards.
- `quests/application/quests_cubit.dart` — claim snackbar updated
  from "your energy will update" to "your inventories will update"
  to reflect dual-reward semantics.

**Test reshape:**
- `test/features/admin_quests/application/admin_quests_cubit_test.dart`
  — `_oneDailyDef` payload + Create + Update calls switched to
  `energyReward` + `hintReward`.
- `test/features/admin_quests/data/admin_quests_repository_test.dart`
  — fetch decode payload + Create + Update body expectations
  switched.
- `test/features/admin_quests/presentation/admin_quests_screen_test.dart`
  — `_oneDailyDef` payload switched.
- `test/features/quests/application/quests_cubit_test.dart` —
  `_readyQuest` + `_claimedQuest` fixtures switched; claim snackbar
  expectation updated.
- `test/features/quests/data/quest_repository_test.dart` — fetch
  decode fixtures switched + new field assertions added.

Detailed Sprint H shape lives in `frontendActiveContext.md > Sprint
H — Hint Module + Quest Multi-Reward (frontend leg)`.

---

## Slice Q1.6 — Quests redesign frontend reshape ✅ closed (2026-05-24, commit a754301)

Frontend leg of the Sprint Q1 redesign. Shipped as a single bundled
commit covering data + application + presentation + tests across both
the admin quest catalog screen and the player quest list. Quality
gate: **103/103 Flutter tests pass**, `flutter analyze` only
pre-existing info-level warnings.

The closed `AdminQuestType` catalog (7 values incl Custom1/2/3) is
gone. The admin form now collects free-text Name + Description, a
fixed Trigger dropdown (3 values), Threshold + Reward numeric inputs,
a Prerequisite dropdown populated from other active definitions, and
a ProgressBaseline dropdown that surfaces only when the trigger is
`gameCompletedTotal`. Name + Trigger are read-only in Edit mode
(server rejects changes per Q1.5 immutability rule).

Player quest tile renders the admin-defined `Name` (title) +
`Description` (subtitle) with no hardcoded quest-type → human-copy
mapping. State badge labels switched to Türkçe ("Hazır", "Aktif",
"Alındı"). `QuestState` shrunk to 4 values — `expired` removed
because the Q1.3 backend deletes expired daily rows on sync rather
than transitioning them.

5 quest-area test files reshaped. Turkish characters in JSON test
fixtures (`'İlk Oyun'`) were replaced with ASCII because
`http.Response` in Flutter test defaults to Latin-1 encoding and
`İ` (U+0130) blows up there. Production wire format stays UTF-8.

Detailed Q1.6 shape and decision rationale live in
`frontendActiveContext.md > Sprint Q1.6 — Quests redesign (frontend
reshape)`.

---

## Admin frontend sprint (F1–F6) ✅ closed (2026-05-22…23)

Backend Administration sprint (B1–B10) kapandıktan sonra altı slice'lık
admin shell. Sprint kapanışında 103/103 test pass, flutter analyze 5
pre-existing info, flutter build web ok.

### Slice F1 — Admin login + ayrı session (2026-05-22, commit 70031bf)

- `features/admin_auth/` yeni feature klasörü:
  - `data/admin_session.dart` — AdminSession DTO (adminUserId, email,
    role, accessToken, expiresAt).
  - `data/admin_token_store.dart` — `SharedPreferencesAdminTokenStore`,
    key `lexilink.admin.accessToken`. Player session ile orthogonal —
    her ikisi farklı `TokenStore` impl'ine sahip.
  - `data/admin_auth_repository.dart` — `POST /auth/admin/token` çağrısı.
  - `application/admin_session_cubit.dart` — checking / unauthenticated
    / authenticating / authenticated / failure state machine.
  - `presentation/admin_login_screen.dart` — `/admin/login` route. Email
    + external token form, dev verifier `dev:admin:{email}`. Success →
    `context.go('/admin')`.
- Router: `/admin/login` + `/admin` rotaları eklendi.
- Tests: admin_auth_repository (happy + 401) + admin_session_cubit
  (checkSession, signIn, signOut, 401/404 friendly messages). 53/53.

### Slice F2 — Admin shell + ShellRoute nav (2026-05-22, commit f57cede)

- `features/admin/presentation/app_admin_shell.dart` — NavigationRail
  (>= 600 px) / NavigationDrawer (mobile) ile dört destination
  (Quests / Players / Energy / Audit). Sign-out admin token store'unu
  temizler ve `/admin/login`'e gider; player session etkilenmez.
- Router refactor: `/admin/{quests,players,energy,audit}` ShellRoute
  altında. `/admin` → `/admin/quests` default redirect. Router-level
  guard: `/admin/*` için token persistsiz ise login'e.
- F1'in geçici `AdminHomeScreen` placeholder'ı silindi.
- Tests: 4 widget smoke (wide rail, narrow drawer, destination
  navigation, sign-out flow). 57/57.

### Slice F3 — Admin quest catalog CRUD (2026-05-22, commit c7847ae)

- `features/admin_quests/`:
  - `data/quest_definition.dart` + `quest_enums.dart` — DTO + wire-form
    enum (`AdminQuestType.firstGameCompleted('FirstGameCompleted')`,
    vb.) server'ın `JsonStringEnumConverter` formatıyla eşleşiyor.
  - `data/admin_quests_repository.dart` — fetch / create / update /
    deactivate.
  - `application/admin_quests_cubit.dart` — load + create / update /
    deactivate, her mutation sonrası reload.
  - `presentation/admin_quests_screen.dart` + `quest_definition_form.dart`
    — Card-list (her satırda cadence + inactive badge), FAB → form
    dialog (Create), per-row Edit + Deactivate.
- `ApiClient.putJson` eklendi.
- Tests: repository (4 endpoint), cubit (load happy/failure + 3
  mutation), screen smoke (list + FAB + empty state).
- F2 shell testleri retargeted: gerçek admin sayfaları yerine stub
  `Scaffold` destination'ları (her slice'ta yeniden hedeflemeden
  kurtarır). 68/68.

### Slice F4 — Admin player console (2026-05-22, commit a31622b)

- `features/admin_players/`:
  - `data/player_admin_detail.dart` — DTO mirror.
  - `data/admin_players_repository.dart` — fetchDetail / ban / unban.
  - `application/admin_players_cubit.dart` — lookup → detail; 404 ayrı
    `notFound` status; mutation sonrası reload.
  - `presentation/admin_players_screen.dart` — GUID input + Look-up
    button; detail card (avatar, handle#discriminator selectable,
    banned/guest badges); contextual primary action (Ban / Unban) +
    confirmation dialogs.
- Kapsam notu: search-by-handle yok — Players modülünde
  `GET /admin/players/{id}` dışında query yok. Backend slice (B-X)
  olarak ayrı tutuldu, frontend-only filtreyle örtülmedi.
- Tests: repository (4 path), cubit (5 senaryo), screen smoke (initial
  / lookup / notFound). 80/80.

### Slice F5 + B11 — Admin energy console (2026-05-22, commit 45f8ac0)

- Backend B11: `GET /admin/players/{playerId}/energy` — passthrough
  endpoint reusing `GetPlayerEnergyQuery` under `AuthenticatedAdmin`.
- Frontend:
  - `features/admin_energy/`:
    - `data/player_energy_snapshot.dart` — PlayerEnergySnapshotDto
      mirror.
    - `data/admin_energy_repository.dart` — fetch / setAmount / grant /
      reset.
    - `application/admin_energy_cubit.dart` — lookup, set / grant /
      reset (her birinde snapshot reload). 404 → `notFound`.
    - `presentation/admin_energy_screen.dart` — GUID lookup, energy
      card (current / max, Full / Over max badge, regen info), Set /
      Grant / Reset butonları + int-input dialogs. Grant copy
      over-max'in kasıtlı olduğunu açıkça yazıyor
      (`PlayerEnergy.GrantBonus` semantiğiyle hizalı).
- Tests: 93/93.

### Slice F6 — Admin audit log (2026-05-23, commit 5d8e75b)

- `features/admin_audit/`:
  - `data/admin_action.dart` — AdminActionDto mirror.
  - `data/admin_audit_repository.dart` — `fetch(adminUserId?, targetType?,
    targetId?, offset, limit)`; null/empty filter key'leri query
    string'den çıkarılır (server'ın string.IsNullOrWhiteSpace
    coalescing davranışıyla hizalı).
  - `application/admin_audit_cubit.dart` — load / applyFilter (offset
    sıfırlar) / nextPage / prevPage. `hasMore` server'da total-count
    olmadığı için `returned.length == pageSize` yaklaşımıyla
    çıkarılır.
  - `presentation/admin_audit_screen.dart` — filter row (admin id /
    target type / target id) + Apply / Clear. Card list (action type,
    occurred-on, target type/id, admin id). Per-row "View payload"
    pretty-printed JSON dialog.
- Shared `AdminPlaceholderPage` kaldırıldı — dört destination da artık
  gerçek ekranlara çözülüyor. Route-target wrapper'ları
  `admin_placeholder_page.dart`'da kalır.
- Tests: 103/103.

### Manual-test follow-on fixes (2026-05-22…23, uncommitted at doc time)

These addressed real bugs / gaps surfaced during end-to-end manual
testing of F1–F6. They ship either as an interim "frontend stabilization"
commit or get absorbed into Sprint Q1.6.

- **Splash deep-link fix** — `usePathUrlStrategy()` in `lib/main.dart`
  before `runApp`. `flutter_web_plugins` added to pubspec. Address-bar
  `/admin/login` now resolves directly; previously bounced through
  splash → `/home`.
- **Player guest flow JWT exchange** —
  `GuestPlayerRepository.registerGuest` now also calls
  `POST /auth/token` after `POST /players/guest`, returning
  `GuestSession(playerId, accessToken)`. `GuestEntryCubit` and
  `SessionCubit.setAuthenticated` updated to carry both. Pre-existing
  anti-pattern (use `playerId` as the bearer) only worked in
  `DevelopmentBearer` mode and silently broke under `ProductionJwt`.
- **TokenStore.savePlayerId / readPlayerId** — separate persisted
  key `lexilink.playerId`. `GameStartCubit` and
  `ProfileSummaryCubit` now read `playerId` from the dedicated API
  (no longer mis-read the access token as the player ID).
- **`useRootNavigator: false` on every admin showDialog** — go_router
  16 + ShellRoute makes default `Navigator.pop` trip a "popped last
  page" assertion that blanks the shell. Override applied across
  quests / players / energy / audit dialogs.
- **Admin energy card stays visible during saving** — Old behavior
  replaced the entire card with a spinner so the operator never saw
  the value flip; new build keeps the card mounted with a dimmed
  Stack + small spinner during the saving state.
- **Quest create dropdown "(exists)" + disabled** — taken types
  shown but disabled; if all types are taken (which is the case with
  the seeded 4 + Custom1/2/3 enum), an inline message points the
  operator to Edit / Deactivate / Reactivate.
- **`AdminQuestType.custom1/2/3` mirror values** — corresponds to
  backend `QuestType.Custom1/2/3` placeholder enum values. Both will
  be removed in Q1.1 / Q1.6.
- **Quest reactivate icon** — inactive rows show a power-icon
  reactivate button; wired to backend B12
  (`POST /admin/quests/definitions/{id}/reactivate`).
- **Player /quests/me deactivated filter** (backend) makes
  deactivated definitions disappear from the player view without
  deleting the underlying PlayerQuests rows. Reactivate brings them
  back. Existing claim history is preserved.

---

## Frontend Planning Started (2026-05-13)

### Slice 11 — Game Screen Polish (2026-05-17)

Game ekraninin Slice 10 ile gelen tasarim diline tasinmasi. Backend
prerequisite (`GET /games/{id}/options`, deterministik 6'li alt küme,
previousLinkId her zaman kilitli) ayni gun sabah `activeContext.md`'de
canli oldugu icin slice acildi.

- **Repository migration** — `lib/features/game/data/game_repository.dart`.
  `getOutgoingLinks(linkId)` legacy yolu silindi; `getOptions(gameId)` tek
  seçenek kaynagi. Cubit zaten `getOptions` cagiriyordu; method dead code'du.
- **DTO sertlestirme** — `lib/features/game/data/game_details.dart`.
  Backend `startLinkId` + `targetLinkId` donduruyordu ama DTO parse
  etmiyordu. Iki alan eklendi; daha onceden value-eslestirmesi ile yapilan
  previous-link tespiti artik id eslestirmesiyle yapiliyor.
- **AppBackBar genisletmesi** — `lib/shared/widgets/app_back_bar.dart`.
  Opsiyonel `onBack` callback ve `trailing` widget parametreleri eklendi;
  null default'lar ile mevcut profile/quests/leaderboard kullanimi
  bozulmadi.
- **GameScreen tam yeniden yazim** —
  `lib/features/game/presentation/game_screen.dart`.
  - `AppBackBar(title: 'Game', onBack: ..., trailing: PopupMenuButton)`.
    Back basili → "Quit game?" `AlertDialog` (Keep playing / Quit) →
    confirm ise `GameDetailsCubit.abandon()`. Game `isFinished` iken back
    direkt `/home`. Overflow popup tek menuy ogesi: "Reset progress (n)"
    (busy/finished/`resetsLeft==0` iken disabled).
  - `_StartRailTarget` = Row(`_AnchorChip(start, primarySoft)`,
    `Expanded(_StepDots)`, `_AnchorChip(target, focusSoft)`). Anchor max
    120px genislik, `caption + label`. Step dots `LayoutBuilder` ile
    `maxSteps` adet 10x10 dot; ilk `stepsTaken` tanesi
    `AppPalette.focus`, kalan `AppPalette.primary.withValues(alpha: 0.18)`.
  - `_CurrentHero` = gradient `LinearGradient(primary →
    primaryPressed)` + 20px radius + soft shadow. Etiket "CURRENT" +
    altinda `FittedBox(BoxFit.scaleDown)` ile `headlineMedium` current
    word (uzun Turkish word safe).
  - `_Breadcrumb` = `startWord › … › currentWord` (son 3 kelime,
    `bodySmall`, muted, ellipsis).
  - `_StatusRow` = 3 chip: Steps `n/m` (timer icon), Hints `n`
    (lightbulb), Score `n` (star, sandy gold ton) — Score yalnizca
    `game.score != null`.
  - `_OptionsGrid` = `GridView.builder(crossAxisCount: 3,
    childAspectRatio: 1.6, mainAxis/crossAxis spacing 10)`. Tile state'leri:
    - normal: light surface, primary 0.28 alpha border;
    - hint-recommended (`option.id == recommendedLinkId`): focusSoft bg,
      sandy gold 2px border, w700 weight;
    - previous (`option.id == previousLinkId`): muted surface + muted
      border + sag-ust kose `Icons.undo` 14px (geri don rozeti);
    - disabled: muted (busy / `!isActive` / `isFinished`).
    Long Turkish word: `FittedBox(BoxFit.scaleDown)`, `maxLines: 2`.
    Tap → `GameDetailsCubit.makeStep(option.id)`.
  - `_SecondaryActions` = Row of `Hint (n)` + `Undo (n)` `AppSecondaryButton`
    (kalan sayilar etikette).
  - Result `showModalBottomSheet(isScrollControlled: true,
    backgroundColor: transparent)`. `BlocListener.listenWhen` only fires
    on `!wasFinished && isFinished`; `_resultShown` flag prevents
    re-open. Sheet: drag handle + outcome icon/title (Completed
    `emoji_events_outlined`/success; Failed `do_not_disturb_alt_outlined`
    /danger; Abandoned `flag_outlined`/muted) + subtitle + 3 summary
    stats (Score, Steps, Hints used) + Path satiri (full
    `startWord › … › currentWord`) + `Back to home` primary CTA.
- **Cleanup** — `widgets/link_tile.dart` ve `widgets/game_info_card.dart`
  artik kullanilmiyor; silindi. `presentation/widgets/` klasoru bos
  oldugu icin kaldirildi.
- **Test guncellemesi** —
  `test/features/game/data/game_repository_test.dart`: `'gets outgoing
  links'` test'i `'gets game options'` olarak yeniden yazildi (yeni path:
  `/games/game-1/options`). `test/features/game/data/game_details_test.dart`
  fixture `startLinkId`/`targetLinkId` ile guncellendi.

Previous-link tespit kurali (`_previousLinkId(game)`):

- `stepsTaken == 0` → null (henuz adim atilmadi);
- `stepsTaken == 1` → `game.startLinkId`;
- `stepsTaken >= 2` → `game.history[stepsTaken - 2].linkId`.

Backend `/games/{id}/options` previousLinkId'yi her zaman alt kümeye dahil
ediyor; frontend bu tile'i muted + undo rozetiyle isaretler.

Out-of-scope (bilincli):

- "Play again" tek tikla yeni oyun yaratma (sheet'te yok; CTA `/home`'a
  donuyor, kullanici splash → home akisindan tekrar baslar).
- Energy badge'ini game ekraninda gostermek (energy oyun basinda
  harcanmis, oyun ortasinda etkilesimsiz; dikkat dagitir).
- Sign-out / Reset session UI (token clear; bug raporu sirasinda ortaya
  cikti — Slice 12+ adayi).
- Path history persistence (oyun browser refresh sonrasi restore yok).

Verification:

- `flutter analyze` 0 yeni issue (yalnizca Slice 10'dan kalan splash
  `prefer_int_literals` info: `splash_screen.dart:152`, blocking degil).
- `flutter test` 45/45 (test sayisi degismedi; bir test fixture'i
  guncellendi, biri yeniden yazildi).
- Live API smoke: `POST /players/guest` (SmokePlayer) → `POST /games`
  → `POST /games/{id}/start` → `GET /games/{id}` (`kış sporu` start, 9
  step budget) → `GET /games/{id}/options` 6 deterministik secenek
  (spor, hokey, snowboard, kar, buz, buz pateni) dondu.
- Browser smoke kullanici tarafindan yapilacak; fresh `flutter run -d
  web-server --web-port 5173 --dart-define=LEXILINK_API_BASE_URL=
  http://127.0.0.1:5099` ile sunucu yeniden baslatildi.

### Slice 10 — Home Landing UX (2026-05-16)

Kullanici-facing girisin tasarim dili polish'i. Eski Bootstrap design-system
preview ekrani ve `/guest` interstitial'i emekli; akis tek hat oldu:
`/` → `SplashScreen` → `/home` (yeni).

- **Splash (sand-pour logo)** — `lib/features/splash/presentation/splash_screen.dart`.
  Harf bazli stagger (`_letterStagger: 220ms`, `_letterDuration: 780ms`)
  ile her harf yukseklik tween'i + opaklik fade-in ile yerlesiyor. Her
  harfin uzerinde `CustomPaint` (`_SandGrainPainter`) deterministic seed'li
  rastgele partiküller bell-curve (`sin(progress·π)`) ile akiyor. Animasyon
  tamamlandiktan 480ms sonra `context.go('/home')`. Renkler `AppPalette.focus`
  (sandy gold).
- **HomeScreen layout** — `lib/features/home/presentation/home_screen.dart`.
  `_HomeScreen` FutureBuilder ile TokenStore'u hazirliyor, `_HomeProviders`
  Session/GuestEntry/CategoryList/Energy/GameStart cubit'lerini sahipleniyor.
  `_HomeView` MultiBlocListener ile `SessionStatus.unauthenticated` →
  `continueAsGuest` (silent), `authenticated` → loadCategories + loadEnergy,
  `GameStart.success` → `/games/:id` yonlendirir.
- **HomeScaffold geometri** — `Stack` icinde `Positioned.fill(Padding(76,64,16,24))`
  ana ic icerik; `Positioned(top:12,right:12)` `EnergyBadge`; `Positioned(top:12,
  left:12)` profile + quests yuvarlak `_SideIconButton`. Ic icerik
  `Align(Alignment(0, -0.18))` ile dikey merkezden hafif yukarida.
- **Swipeable kategori carousel** — `ConstrainedBox(maxWidth: 420)` +
  `LayoutBuilder` ile `cardSize = constraints.maxWidth * 0.82` hesaplanir.
  PageView height da `cardSize` (kare kart). `PageController(viewportFraction:
  0.82)`; `ScrollConfiguration(_DragScrollBehavior)` Flutter web'de
  `PointerDeviceKind.mouse/touch/stylus/trackpad` drag'i aciyor. Aktif olmayan
  kartlar `AnimatedBuilder` ile `Transform.scale(1 - distance * 0.06)`
  ile minor sekilde küçülüyor. `_PageDots` aktif sayfayi 22px stretch,
  diger sayfalari 8px nokta ile gosteriyor.
- **Kategori kart gorseli** — `_CategoryCard` gradient (LinearGradient
  topLeft→bottomRight) + 20px radius + soft shadow (alpha 0.22, blur 14).
  Sol-ust beyaz `titleLarge` baslik, ortada 72px emoji. Emoji + gradient
  `_categoryVisuals(name)` map'i ile uretilir (Hayvanlar 🦊 yesil; Yemekler 🍜
  turuncu; Doğa 🌿 mavi; default 🎲 primary). Backend `Category` modelinde
  `imageUrl` yok; gercek resim ihtiyaca gore bir sonraki slice.
- **Logo** — `_LexiLinkWordmark` `headlineMedium` + AppPalette.focus + bold,
  letter-spacing 1.2. `SizedBox(width: cardSize * 0.72)` + `FittedBox(BoxFit.
  fitWidth)` ile her zaman kartin yaklasik dortte ucu kadar genis.
- **Start aksiyonu** — Carousel altinda `cardSize` genisliginde
  `AppPrimaryButton`; secili (current page) kategori ile
  `GameStartCubit.startGame`'i tetikliyor. `GameStart.failure` durumunda
  `EnergyCubit.loadEnergy` reload.
- **Sahte content seed (gecici)** — Dev DB'de kategori yoktu; `POST
  /players/guest` ile seed player olusturulup `Authorization: Bearer <id>`
  ile `POST /categories` uc kez cagrildi: Hayvanlar, Yemekler, Doğa.
  Production icin ayrica content import islemi gerekecek.
- **Shared AppBackBar** — `lib/shared/widgets/app_back_bar.dart`. Yuvarlak
  geri butonu (Material elevation 1) + ortalama olmayan `titleLarge` baslik;
  `fallbackRoute` default `/home`. Profile, Quests, Leaderboard ust basligi
  bu widget'a cevrildi; alttaki "Back" butonlari ve `context.go('/guest')`
  cagrilari silindi.
- **Profile polish** — Avatar 84x84 dairesel sandy-gold gradient + ortada
  baş harf. Handle ortali, "provider · locale" alt satir. Stats karti
  16px radius + ince divider ile satir ayrimi. Alttaki Back butonu silindi.
- **Quests polish** — Quest tile'lara `ClipRRect(LinearProgressIndicator)`
  eklendi (`progress / goal` ile). "Progress N/M" sola, "+N⚡" sağa
  spaceBetween yerlesim. Üst başlık `AppBackBar` ile yeniden formatlandi.
- **Leaderboard polish** — Ust ozet `AppBackBar` ile sadelesti; alttaki
  Back butonu silindi (SegmentedButton yerinde).
- **Cleanup** — `/guest` route'u kaldirildi.
  `lib/features/auth/presentation/guest_entry_screen.dart` silindi.
  `lib/features/bootstrap/` (cubit + screen + test) tamamen silindi.
  `test/app/app_test.dart` `SplashScreen` render kontrolune cevrildi.
  `GuestEntryCubit` + `GuestPlayerRepository` yerinde kaldi (HomeScreen
  silent auth icin kullaniyor).

Out-of-scope (bilincli):

- Kategori icin gercek `imageUrl` alani (backend Domain + DbUp migration
  gerektirir; emoji map'i simdilik yeterli).
- Splash → home arası animatlı geçiş (`context.go` instant transition).
- `/categories` legacy route'unun silinmesi (bir sonraki cleanup'ta).
- Game screen tasarim dili pass'i (Slice 11 adayi).

Verification:

- `flutter analyze` 0 issue (1 info: `prefer_int_literals` —
  `splash_screen.dart:152`, blocking degil).
- `flutter test` 45/45 (eski 46'dan 45'e dustu; eski Bootstrap widget
  testi silindi, `app_test.dart` SplashScreen render kontrolune cevrildi).
- Chrome smoke `localhost:5173`: splash animasyonu, carousel mouse drag,
  side icon navigation, AppBackBar geri donus ile `/home` manuel
  dogrulandi.

### Slice 9 — Quests on frontend (2026-05-15)

LexiLink frontend'in besinci feature'i (Energy sonrasi). Backend'in
2026-05-15'te shipped Quests modulunu (`GET /quests/me`,
`POST /quests/{id}/claim`) UI tarafinda kullanilabilir hale getirir.

- **Data layer** — `features/quests/data/`. `PlayerQuest` DTO
  (id, playerId, questType, state, progress, goal, rewardAmount, issuedAt,
  completedAt?, claimedAt?, expiresAt?) + `QuestState` enum
  (active/readyToClaim/claimed/expired/unknown) `fromString` parse'i ile.
  `QuestRepository.getMe()` `GET /quests/me` listesini DTO'lara cevirir;
  `claim(id)` `POST /quests/{id}/claim` cagrisi yapar (204 NoContent;
  `ApiClient._decodeObject` empty body → `{}` ile uyumlu).
- **Application layer** — `QuestsCubit` (initial/loading/success/failure).
  `loadQuests()` + `claim(id)` + `clearClaimMessage()`. Claim state'inde
  `claimingId` per-tile spinner icin tutuluyor; ayni anda iki claim
  yapilamaz. Claim basarili olursa quest listesi reload olur ve
  `claimMessage` "Reward queued — your energy will update in a few seconds"
  ile set edilir; reload hatasi swallow edilir (claim zaten basarili).
  Hata path'i `ApiException.message` mesaji ile claim message'i ayarlar.
- **Presentation layer** — `/quests` route + `QuestsScreen`. Tile listesi
  state oncelikli sirayla (Ready → Active → Claimed → Expired, sonra
  issuedAt DESC). Her tile insanca quest type adi + state badge +
  "Progress N/M · reward +K⚡" + ReadyToClaim icin Claim butonu (busy
  state spinner). Claim basarili olunca SnackBar ile reward queued mesaji
  gosterilir, mesaj `clearClaimMessage()` ile state'ten temizlenir.
  Loading/error/empty state'leri shared widget'lardan geliyor.
- **Navigation** — Guest ready ekraninda "View quests" sekonder buton;
  `/quests` ekraninda "Back" butonu `/guest`'e geri dondurur.
- **Reward async UX kararı** — Reward (bonus energy) backend outbox →
  Energy GrantBonus akisi ile asenkron geliyor (~5s polling). UI claim
  sonrasi quest listesini reload eder ve kullaniciya reward queued
  bilgisini SnackBar ile verir. Aktif energy polling/animasyonu yok;
  energy badge bir sonraki ekran ziyaretinde guncel goruluyor.
- **Tests** — `quest_repository_test.dart` (2 test: list parse + claim
  POST path), `quests_cubit_test.dart` (4 test: load happy + 401 failure
  mapping + claim reload + claim 404 hata path'i).
- **Verification** —
  - `flutter analyze` → 0 issue.
  - `flutter test` → 46/46 (eski 40 + 6 yeni Quests).
  - `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
    basarili.
  - Live API smoke (DevelopmentBearer): yeni guest icin `GET /quests/me`
    `[]` 200 dondurdu; `POST /quests/{fake-id}/claim` 404 ProblemDetails;
    bearer-less claim 401.
- **Non-goals** — quest progress live tick animasyonu, daily reset
  countdown UI, push notification, reward arrival icin aktif energy
  polling.

### Slice 8 — Energy badge on frontend (2026-05-14)

- **Data layer** — Added `PlayerEnergy` DTO (parses `playerId`, current/max,
  `isFull`, recharge interval, `lastRefilledOn`, `secondsUntilNextRefill`,
  `fullyRefilledAt`) and `EnergyRepository.getMe()` for `GET /energy/me`.
- **Energy state** — Added `EnergyCubit` with initial/loading/success/failure
  states; reuses `ApiException` → message mapping. No live countdown timer;
  the snapshot value from the last fetch is shown.
- **Badge widget** — Added `EnergyBadge` pill widget with a bolt icon,
  `currentAmount/maximumAmount`, and either `Full` or `Next in Xm Ys` countdown
  text. Loading and failure variants reuse the pill shell.
- **Guest ready screen** — Hosts `EnergyCubit` and shows the badge inside the
  success panel. A `BlocListener<SessionCubit>` triggers `loadEnergy()` when
  session flips to authenticated.
- **Category selection screen** — Hosts a second `EnergyCubit`, loads on
  mount, renders the badge above the category list. After `GameStartCubit`
  emits `failure`, the cubit reloads so the badge reflects an insufficient
  or partially-consumed snapshot. After `success`, the screen navigates
  to `/games/:gameId`; on return, the screen re-mounts and the badge is
  fresh.
- **Game start error** — `GameStartCubit.failure.message` already pipes
  `ApiException.message` through; the backend's
  `EnergyMustBeSufficientToConsumeRule` text reaches the user without extra
  wiring.
- **Coverage** — Added repository tests for the non-full and full snapshots,
  plus cubit tests for the success path and the `ApiException` path.
- **Verification** — `flutter analyze` passed; `flutter test` passed 40/40;
  `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
  passed.

### Slice 7d — Daily/weekly leaderboard period selector (2026-05-14)

- **Query model** — `LeaderboardQuery` is now `Equatable` with a `copyWith`,
  so cubit state transitions stay clean when the selected period changes.
- **Cubit** — Added `LeaderboardCubit.changePeriod(LeaderboardPeriod)`. It
  no-ops when the requested period equals the current successful query;
  otherwise it reloads with `state.query.copyWith(period: ...)`. Loading and
  failure states now carry the active query so the selector and retry actions
  stay deterministic.
- **UI** — Added a Material `SegmentedButton` with All-time / Daily / Weekly
  segments on `LeaderboardScreen`. The selector is disabled while loading. The
  subtitle and empty-state message adapt to the selected period (UTC day vs.
  Monday-start UTC week).
- **Coverage** — Added cubit tests for changePeriod reloading with the new
  period and changePeriod no-op when the period is unchanged.
- **Live API smoke** — Against the running local API:
  `period=allTime` returned 2 entries, `period=daily` returned 0, and
  `period=weekly` returned 2 (the same 2 since recent games landed this week).
- **Verification** — `flutter analyze` passed; `flutter test` passed 36/36;
  `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
  passed.

### Slice 7c — All-time leaderboard presentation baseline (2026-05-14)

- **Leaderboard state** — Added `LeaderboardCubit` with initial, loading,
  success, and failure states. Defaults to all-time `bestScore` ordering through
  `LeaderboardQuery`; the resolved query is kept on the state so future period
  selectors can reuse it.
- **Leaderboard screen** — Added `/leaderboard` route and `LeaderboardScreen`.
  Each row shows rank, handle, games completed, total score, and best score.
  Loading, error (with retry), and empty (with refresh) states reuse the shared
  widgets.
- **Navigation** — Guest ready screen now links to the leaderboard alongside
  the profile entry; profile summary screen also links to the leaderboard.
- **Coverage** — Added cubit tests for the parsed-entries success path, the
  empty-list success path, and the API error path through `ApiException`.
- **Live API smoke** — `GET /stats/leaderboard?orderBy=bestScore&period=allTime`
  against the running local API returned the expected camelCase entries
  including the new guest player session.
- **Verification** — `flutter analyze` passed; `flutter test` passed 34/34;
  `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
  passed.

### Slice 7b — Profile summary presentation baseline (2026-05-14)

- **Profile state** — Added `ProfileSummaryCubit` with initial, loading,
  success, and failure states; reads the authenticated guest player id from the
  shared token store and loads `GET /stats/players/{playerId}` through
  `PlayerStatsRepository`.
- **Profile screen** — Added `/profile` route and `ProfileSummaryScreen`. The
  screen renders the handle, provider/locale summary, games completed, best
  score, total score, and last completed date. Loading, error (with retry), and
  empty states reuse the shared widgets.
- **Navigation** — Guest ready screen now links to the profile summary screen
  in addition to category selection.
- **Coverage** — Added cubit tests for the session-present success path, the
  missing-session path, and the API error path through `ApiException`.
- **Verification** — `flutter analyze` passed; `flutter test` passed 31/31;
  `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
  passed.

### Slice 7a — Stats contract data layer (2026-05-13)

- **Backend contract** — Read Stats endpoints:
  `GET /stats/players/{playerId}` and `GET /stats/leaderboard`.
- **Profile stats model** — Added `PlayerStats` for profile summary fields:
  handle, avatar, locale, guest status, linked providers, completed games,
  scores, and timestamps.
- **Leaderboard model** — Added `LeaderboardEntry`, `LeaderboardOrderBy`,
  `LeaderboardPeriod`, and `LeaderboardQuery`.
- **Repository** — Added `PlayerStatsRepository` for player stats and leaderboard
  API calls.
- **Coverage** — Added repository tests for player stats parsing, leaderboard
  query parameters, and leaderboard entry parsing.
- **Live API smoke** — Local API returned 200 for player stats and all-time
  leaderboard with the expected camelCase contract.
- **Verification** — `flutter analyze` passed; `flutter test` passed 28/28;
  `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
  passed.

### Slice 6c — Completed/failed game smoke and polish (2026-05-13)

- **Game details parsing** — `GameDetails` now parses nullable backend `score`
  and `history` items.
- **Game UI polish** — `GameScreen` shows score when available and renders the
  played path history from start through submitted steps.
- **Coverage** — Added a completed game details parsing test covering score and
  history.
- **Completed smoke** — Played local API game
  `boks -> dövüş sporu -> spor -> antrenman`; backend returned
  `state: Completed`, `stepsTaken: 3`, `score: 300`.
- **Failed smoke** — Played local API game
  `pota -> basket -> ekipman -> ayakkabı -> bisiklet -> ayakkabı -> bisiklet ->
  ayakkabı -> bisiklet`; backend returned `state: Failed`, `stepsTaken: 8`,
  `score: null`.
- **Verification** — `flutter analyze` passed; `flutter test` passed 26/26;
  `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
  passed.

### Slice 5a — Category selection baseline (2026-05-13)

- **Backend contract** — Read `GET /categories`; endpoint is authenticated and
  returns a JSON list of `{ id, name }`.
- **API client** — Added JSON list response decoding to `ApiClient`.
- **Category data layer** — Added `Category` DTO and `CategoryRepository`.
- **Category state** — Added `CategoryListCubit` with loading, success,
  failure, and selected-category state.
- **Category UI** — Added `/categories` route and `CategorySelectionScreen`
  with loading/error/empty states and selectable category tiles.
- **Guest navigation** — Guest ready screen now links to category selection.
- **Backend smoke** — Authenticated `GET /categories` against local API returned
  200. Current dev DB category list is empty, so the screen shows empty state.
- **Verification** — `flutter analyze` passed; `flutter test` passed 18/18;
  `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
  passed.

### Slice 5b — Spor content import (2026-05-13)

- **Source file** — Read `docs/category-spor.json`; validated 157 unique links,
  1234 unique directed edges, and no edge references outside the link list.
- **Importer** — Added `LexiLink.Tools.CategoryImporter`, a small .NET tool
  that imports category JSON directly into PostgreSQL with deterministic ids and
  upsert behavior.
- **Database import** — Imported category `Spor` into local PostgreSQL:
  category id `f29ec5db-774d-eb3b-9974-6fbecfbecf6d`.
- **API verification** — Authenticated `/categories` returns Spor;
  `/categories/{id}` returns `linkCount: 157`; `/links?categoryId=...` returns
  157 links.
- **Verification** — `dotnet build
  src/Tools/LexiLink.Tools.CategoryImporter/LexiLink.Tools.CategoryImporter.csproj`
  passed with 0 warnings/errors.

### Slice 5c — Start Game baseline (2026-05-13)

- **Quick-start decision** — Category selection starts an Easy game directly;
  category detail is deferred until the game flow needs richer category
  metadata.
- **Game repository** — Added create/start/get game API calls for
  `POST /games`, `POST /games/{id}/start`, and `GET /games/{id}`.
- **Game start state** — Added `GameStartCubit`; it reads the guest player id
  from session token storage, creates a game, starts it, and emits the game id.
- **Game screen** — Added `/games/:gameId` route and first game screen showing
  start/current/target words, steps left, hints remaining, difficulty, and
  backend state.
- **Backend smoke** — Created an Easy game with Spor category, started it, and
  loaded details from local API. Backend returned start `lig`, target
  `antrenman`, current `lig`, maxSteps 8, hintsTotal 3.
- **Verification** — `flutter analyze` passed; `flutter test` passed 21/21;
  `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
  passed.

### Slice 6a — Outgoing choices and make step (2026-05-13)

- **Outgoing choices** — `GameRepository` now loads
  `/links/{currentLinkId}/outgoing`; `GameScreen` renders active outgoing links
  as selectable link tiles.
- **Make step** — Added `POST /games/{id}/steps` support. Selecting a choice
  submits the step, reloads game details, and reloads outgoing choices for the
  new current link.
- **Game state** — `GameDetailsCubit` now owns game details, outgoing choices,
  step submission state, and recoverable step errors.
- **Backend smoke** — Loaded outgoing choices for current word `lig`, submitted
  the `kulup` step, and reloaded game details. Backend returned
  `currentWord: kulup`, `stepsTaken: 1`, `state: InProgress`.
- **Verification** — `flutter analyze` passed; `flutter test` passed 23/23;
  `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
  passed.

### Slice 6b — Game controls and result states (2026-05-13)

- **Game controls** — Added Hint, Undo, Reset, and Abandon controls to
  `GameScreen`.
- **Repository support** — Added `POST /games/{id}/hint`, `/undo`, `/reset`,
  and `/abandon` calls. Hint responses are parsed and used to highlight the
  recommended outgoing choice.
- **State handling** — `GameDetailsCubit` now tracks active action state,
  recommended hint link, action errors, and reloads game details/outgoing
  choices after each successful action.
- **Result states** — Completed, Failed, and Abandoned games show a result
  panel and disable further step/control actions.
- **Backend smoke** — Verified hint, undo, reset, and abandon against local API.
  Reset returns 400 when no step exists, then succeeds after a step; abandon
  returns game state `Abandoned`.
- **Verification** — `flutter analyze` passed; `flutter test` passed 25/25;
  `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
  passed.

### Slice 4a — Guest entry dev flow (2026-05-13)

- **Guest API contract** — Added `GuestPlayerRepository` for backend `POST
  /players/guest` with device id, display name, and locale payload.
- **Guest state** — Added `GuestEntryCubit` with idle, submitting, success, and
  failure states; successful guest creation writes the returned player id into
  `SessionCubit`.
- **Guest UI** — Added `/guest` route and `GuestEntryScreen`; bootstrap start
  action now navigates to the guest entry flow.
- **Development bearer** — For local/dev backend compatibility, the guest
  player id is used as the temporary bearer token. Production token exchange,
  native sign-in, and persistent secure storage remain deferred.
- **Tests** — Added repository and cubit tests for request payload, returned
  player id, token store write, and authenticated session state.
- **Verification** — `flutter analyze` passed; `flutter test` passed 11/11;
  `flutter build web` passed.

### Slice 4b — Guest session restart and reset (2026-05-13)

- **Persistence decision** — Guest dev session is persisted with
  `shared_preferences` across iOS, Android, and web. This is acceptable for the
  temporary DevelopmentBearer player id, not for future production auth tokens.
- **Async token store** — `TokenStore` now supports async token reads;
  `SharedPreferencesTokenStore` caches the current token and keeps API header
  injection compatible with persisted session state.
- **Restart behavior** — `SessionCubit.checkSession` restores authenticated
  state from persisted token when the app starts.
- **Reset behavior** — Guest ready UI now includes reset local session; reset
  clears persisted token, emits unauthenticated session, and returns to guest
  start state.
- **Tests** — Added coverage for session restore and guest entry reset state.
- **Verification** — `flutter analyze` passed; `flutter test` passed 13/13.

### Slice 4c — Real backend guest smoke (2026-05-13)

- **Database** — Ran DbUp migrator against local PostgreSQL; 0 pending scripts,
  upgrade succeeded.
- **API startup** — Started LexiLink API in Development mode on
  `http://127.0.0.1:5099` with `Authentication__Mode=DevelopmentBearer`.
- **Guest contract** — `POST /players/guest` returned a real player id.
- **Bearer contract** — `GET /players/{id}` with `Authorization: Bearer
  <playerId>` returned player details, proving the frontend dev token strategy
  matches backend DevelopmentBearer behavior.
- **CORS** — Added configured API CORS policy and allowed only the local Flutter
  preview origins in Development. Browser preflight for `/players/guest` now
  returns 204 with the expected allow-origin headers.
- **Frontend build** — Rebuilt Flutter web with
  `LEXILINK_API_BASE_URL=http://127.0.0.1:5099`.
- **Verification** — `dotnet build src/API/LexiLink.API/LexiLink.API.csproj`
  passed with 0 warnings/errors; `flutter analyze` passed; `flutter test`
  passed 13/13; `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
  passed.

### Slice 4d — Guest retry/idempotency fix (2026-05-13)

- **Issue** — A second guest entry attempt with the same frontend device id
  returned HTTP 500 from `POST /players/guest`.
- **Backend fix** — `RegisterGuestPlayerCommandHandler` now first looks up an
  existing `AuthProvider.Guest` identity by device id and returns that player id
  instead of trying to insert a duplicate guest identity.
- **Coverage** — Added Players integration coverage for repeated guest
  registration with the same device id returning the existing player id.
- **Smoke** — Repeated live API `POST /players/guest` calls with
  `frontend-preview-device` now both return the same player id.
- **Verification** — Targeted Players integration test passed; `dotnet build
  src/API/LexiLink.API/LexiLink.API.csproj` passed with 0 warnings/errors;
  `flutter analyze` passed; `flutter test` passed 13/13.

### Slice 3a — API client and session foundation (2026-05-13)

- **API config** — Added `ApiConfig` with base URL, timeout, and path/query URI
  construction. Local default is `http://127.0.0.1:5000`, overridable with
  `LEXILINK_API_BASE_URL` at build time.
- **HTTP client** — Added `ApiClient` with JSON GET/POST, timeout, standard
  JSON headers, and bearer-token header injection.
- **Error mapping** — Added `ApiProblemDetails` and `ApiException`; backend
  `application/problem+json` responses are parsed into frontend error models,
  and 401 maps to an authentication error.
- **Session state** — Added `TokenStore`, `InMemoryTokenStore`, and
  `SessionCubit` with checking, unauthenticated, and authenticated states.
- **Tests** — Added API client tests for bearer headers, ProblemDetails
  parsing, unauthorized mapping, plus SessionCubit tests for check/auth/signout.
- **Verification** — `flutter analyze` passed; `flutter test` passed 8/8;
  `flutter build web` passed.

### Slice 2a — Calm focus color palette (2026-05-13)

- **Palette decision** — Chose a low-glare, alert palette: soft mint/off-white
  surfaces, dark petrol text, calm teal primary, and muted amber focus/accent.
- **Theme implementation** — Added `AppPalette` and wired explicit light/dark
  `ColorScheme` values into `AppTheme`.
- **Bootstrap preview** — Added palette swatches to the bootstrap screen so
  the first visual direction is visible in web/mobile shells.
- **Verification** — `flutter analyze` passed; `flutter test` passed 2/2;
  `flutter build web` passed.

### Slice 2b — Typography, buttons, and screen shell (2026-05-13)

- **Typography** — Added fixed system-font text scale to `AppTheme`: display,
  headline, title, body, and label styles with no viewport-driven font sizing.
- **Buttons** — Added 8px-radius, 48px-min-height button baseline and shared
  `AppPrimaryButton`, `AppSecondaryButton`, and `AppDangerButton` widgets.
- **Screen shell** — Added `AppScreen` with safe area, centered max width, and
  stable padding for mobile/web layout consistency.
- **Bootstrap refactor** — Bootstrap screen now uses `AppScreen` and
  `AppPrimaryButton` so future screens inherit the same design rules.
- **Verification** — `flutter analyze` passed; `flutter test` passed 2/2;
  `flutter build web` passed.

### Slice 2c — State widgets and game visual primitives (2026-05-13)

- **State widgets** — Added `AppLoadingState`, `AppErrorState`, and
  `AppEmptyState` for calm loading, recoverable errors, and non-error empty
  states.
- **Game primitives** — Added `LinkTile` with normal/current/target/disabled
  tones and `GameInfoCard` for compact game metrics.
- **Responsive fix** — Made `AppScreen` scrollable by default after widget
  tests caught preview overflow in the default test viewport.
- **Bootstrap preview** — Added loading, empty, error, link tile, and game info
  previews to the bootstrap screen.
- **Verification** — `flutter analyze` passed; `flutter test` passed 2/2;
  `flutter build web` passed.

### Slice 2d — Responsive layout baseline (2026-05-13)

- **Layout constants** — Added `AppBreakpoints`, `AppSpacing`, `AppLayout`, and
  `AppScreenSize`.
- **Responsive rules** — Mobile is `<600` with 16px screen padding; tablet/web
  uses 24px padding; desktop starts at `>=1024`.
- **Screen presets** — Added compact 560, standard 720, game 840, and wide 960
  max-width presets through `AppScreen`.
- **Bootstrap usage** — Bootstrap preview now uses `AppScreenSize.compact`.
- **Verification** — `flutter analyze` passed; `flutter test` passed 2/2;
  `flutter build web` passed.
- **Result** — Slice 2 Design System Baseline is complete. Next slice is API
  Client And Session Foundation.

### Reference model decision

- **Platform goal** — LexiLink frontend will target iOS, Android, and web from
  one codebase.
- **Language/framework** — Flutter + Dart selected.
- **Idol/reference** — Very Good Ventures / Very Good CLI accepted as the
  practical frontend reference, supported by Flutter official architecture and
  Bloc/Cubit architecture.
- **Architecture stance** — Feature-first Flutter structure, clear UI/business
  logic/data boundaries, repository-based API access, minimal frontend domain
  modeling.
- **Planning docs** — Added `frontendActiveContext.md`,
  `frontendRoadmap.md`, and `frontendProgress.md` so frontend work can proceed
  independently from backend active context.

### Next

- Start Slice 1: Frontend Bootstrap.

### Slice 1a — Manual Flutter source scaffold (2026-05-13)

- **Project shell** — Added `frontend/pubspec.yaml`, `analysis_options.yaml`,
  and `README.md`.
- **Architecture baseline** — Added `lib/app`, `lib/features`, and
  `lib/shared` folders with app/router/theme separation.
- **State management** — Added the first `BootstrapCubit` and Equatable state.
- **UI smoke** — Added a minimal `BootstrapScreen` and widget test.
- **Toolchain blocker** — `flutter` and `dart` are not installed on this
  machine/PATH yet, so `flutter create`, `flutter pub get`, `flutter analyze`,
  and `flutter test` could not be run.

### Slice 1b — Flutter platform scaffold and verification (2026-05-13)

- **Toolchain** — Installed Flutter via Homebrew. Verified Flutter 3.41.9 and
  Dart 3.11.5.
- **Platform scaffold** — Ran `flutter create . --platforms=ios,android,web`
  inside `frontend/`, preserving the LexiLink app source and generating iOS,
  Android, and web platform folders.
- **Dependency baseline** — Resolved Flutter dependencies with `flutter pub
  get`; added `cupertino_icons` to match standard Flutter platform assets.
- **Generated cleanup** — Removed Flutter's generated counter test because the
  app uses `LexiLinkApp` and Bootstrap feature instead.
- **Verification** — `flutter analyze` passed; `flutter test` passed 2/2;
  `flutter build web` passed.
- **Result** — Slice 1 Frontend Bootstrap is complete. Next slice is Design
  System Baseline.
