# frontendRoadmap.md

Frontend icin ileriye donuk plan. Tamamlanan isler `frontendProgress.md`,
su anki odak `frontendActiveContext.md` icindedir.

---

## Frontend Reference Model

LexiLink frontend icin referans model:

- **Flutter official architecture guide** temel mimari rehberdir.
- **Very Good Ventures / Very Good CLI** production-grade proje disiplini icin
  idol kabul edilir.
- **Bloc/Cubit** state management ve business-logic ayrimi icin ana cizgidir.

Bu proje, Kamil backend referansinda oldugu gibi frontend'de de disiplinli ama
pragmatik ilerler. Amac ceremony degil; test edilebilir, okunabilir ve
platformlar arasi tutarli bir oyun deneyimidir.

---

## Slice 1 — Frontend Bootstrap ✅ done

Goal: Flutter projesini iOS, Android ve web hedefleriyle calisir hale getirip
temel mimari kabugu kurmak.

- [x] Flutter app scaffold olustur.
- [x] `app/`, `features/`, `shared/` temel klasor yapisini kur.
- [x] Router ve theme icin minimal app kabugu ekle.
- [x] Bloc/Cubit dependency kararini ekle.
- [x] Lint/analysis baseline'i calistir.
- [x] Ilk widget smoke testini ekle.

Acceptance:

- `flutter analyze` gecmeli.
- `flutter test` gecmeli.
- Web target lokal calismali.
- Mobile target dosyalari scaffold icinde korunmali.

Verification:

- `flutter analyze` passed.
- `flutter test` passed, 2/2.
- `flutter build web` passed.

---

## Slice 2 — Design System Baseline ✅ done

Goal: LexiLink'in oyun kimligini tasiyan ama mobile/web'de sade kalan temel UI
sistemini kurmak.

- [x] Color palette kararini tanimla.
- [x] Typography kararlarini tanimla.
- [x] App theme light/dark baseline kararini ver.
- [x] Ortak button ve screen shell widget'larini ekle.
- [x] Loading, error, empty state widget'lari ekle.
- [x] Game-specific tile/link/card gorsel dili icin ilk componentleri tasarla.
- [x] Responsive layout kurallarini belirle.

Acceptance:

- UI componentleri story/smoke ekraninda gorulebilir.
- Mobil ve web genisliklerinde text overlap olmaz.
- Tek renk ailesine sikismayan dengeli palette kullanilir.

Responsive rules:

- Mobile: width < 600, padding 16.
- Tablet/web: width >= 600, padding 24.
- Desktop: width >= 1024.
- Max widths: compact 560, standard 720, game 840, wide 960.
- `AppScreen` scrollable by default.
- Font sizes do not scale with viewport width; layout and spacing adapt instead.

Verification:

- `flutter analyze` passed.
- `flutter test` passed, 2/2.
- `flutter build web` passed.

---

## Slice 3 — API Client And Session Foundation ✅ done

Goal: Backend'e baglanacak ortak HTTP/session altyapisini kurmak.

- [x] Base API client ekle.
- [x] Environment config: local/dev/prod API base URL.
- [x] Token storage stratejisini sec.
- [x] Authenticated request interceptor/middleware ekle.
- [x] Ortak API error mapping ekle.

Acceptance:

- Fake veya test endpoint ile basarili request test edilir.
- 401/validation/problem-details response'lari frontend error modeline map
  edilir.

Current status:

- Baseline complete with in-memory token storage. Persistent secure storage is
  deferred until Auth / Guest Entry decides mobile/web persistence behavior.
- `flutter analyze`, `flutter test`, and `flutter build web` pass.

---

## Slice 4 — Auth / Guest Entry

Goal: Oyuncunun guest olarak baslayip authenticated session'a hazir hale
gelmesini saglamak.

- [x] Guest start screen.
- [x] Guest player create flow.
- [ ] Token exchange/login UI iskeleti.
- [x] Session Cubit/Bloc integration.
- [x] Logout/reset local session.

Acceptance:

- Guest player akisi UI'da tamamlanir.
- Session state app restart davranisi icin tasarlanir.
- Backend auth contract'i ile uyumlu DTO'lar kullanilir.

Current status:

- Guest dev flow complete: `/guest` screen registers through `POST
  /players/guest`, stores the returned player id as the local dev bearer token,
  and marks `SessionCubit` authenticated.
- Guest restart/reset complete: local guest bearer token is persisted with
  `shared_preferences`; app restart restores authenticated session, and reset
  clears local session.
- Real backend smoke complete: local API on `http://127.0.0.1:5099` accepts
  guest registration and DevelopmentBearer player id auth from the Flutter web
  preview origin.
- Production/native token exchange remains deferred until provider strategy and
  secure storage behavior are finalized.

---

## Slice 5 — Category Selection

Goal: Oyuncunun kategori secip oyun baslatma akisini kurmak.

- [x] Category list screen.
- [x] Category repository.
- [x] Loading/error/empty states.
- [x] Category detail veya quick-start karari.
- [x] Start game action.

Acceptance:

- Category list web/mobile layout'ta okunabilir.
- Start game action UI state'i deterministik olur.

Current status:

- Baseline complete: `/categories` route loads authenticated backend
  `GET /categories`, maps `{id,name}` items, shows loading/error/empty states,
  and supports local selected-category UI state.
- Quick-start selected: selected category starts an Easy game directly.
- Start Game baseline complete: frontend creates, starts, and loads a game,
  then routes to `/games/:gameId`.

---

## Slice 6 — Game Screen MVP ✅ done

Goal: LexiLink'in ana oyun ekranini oynanabilir MVP seviyesine getirmek.

- [x] Current link, target link ve path durumunu goster.
- [x] Outgoing link choices UI.
- [x] Make step action.
- [x] Undo, reset, hint, abandon actions.
- [x] Score/step budget/hint counters.
- [x] Completed/failed/abandoned result states.

Acceptance:

- Bir oyun session'i UI'da baslatilip tamamlanabilir.
- Button/action state'leri duplicate request ve invalid state'e karsi korunur.
- Mobilde tek elle oynanabilir layout hedeflenir.

Current status:

- Game screen loads current/target/start words, outgoing choices, steps left,
  and hints remaining.
- Tapping an outgoing choice posts a step and reloads game details plus the next
  outgoing choices.
- Hint, undo, reset, and abandon are wired to backend actions; finished games
  show a result panel and disable further actions.
- Score and played path history are shown from backend game details.
- Real API smoke covered both `Completed` and `Failed` outcomes.
- `flutter analyze`, `flutter test`, and web build pass.

---

## Slice 8 — Energy On Frontend ✅ done

Goal: backend `GET /energy/me` üzerine ince bir energy görselleştirme katmanı.

- [x] `PlayerEnergy` DTO + `EnergyRepository.getMe()`.
- [x] `EnergyCubit` (initial/loading/success/failure).
- [x] `EnergyBadge` pill widget (bolt icon + `N/M` + `Full` / `Next in …`).
- [x] Guest ready screen wiring (session authenticated tetikler).
- [x] Category selection screen wiring (mount load + game start failure reload).
- [x] Insufficient energy mesajı kullaniciya ulaşıyor (mevcut
  `GameStartCubit.failure.message` üzerinden).
- [x] Repository + cubit testleri.

Acceptance:

- `/energy/me` snapshot guest ready ve category selection ekranlarında
  görünür.
- Insufficient energy backend rejection'ı kullanıcıya backend rule message
  ile ulaşır.
- Game başlatma sonrası dönüldüğünde badge taze veriyle gelir.

Verification:

- `flutter analyze` passed.
- `flutter test` passed 40/40.
- `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
  passed.

Non-goals:

- Live countdown timer (badge sadece son fetch'i gösterir; tick-down
  animasyonu yok).
- Full-screen energy snapshot route (gerek olduğunda eklenir).

---

## Slice 7 — Leaderboard And Profile ✅ done

Goal: Stats ve player profile yuzeyini oyuncuya gostermek.

- [x] Stats contract data layer.
- [x] Player profile summary.
- [x] All-time leaderboard.
- [x] Daily/weekly leaderboard period selector.
- [x] Empty/error states.

Acceptance:

- Backend Stats contract'i ile uyumlu calisir.
- Period secimi UI'da net ve stabil olur.

Current status:

- Data layer baseline complete: `PlayerStatsRepository` reads player stats and
  leaderboard endpoints; DTO parsing and leaderboard query params are covered by
  tests.
- Profile summary presentation baseline complete: `ProfileSummaryCubit` and
  `/profile` route load `GET /stats/players/{playerId}` for the current guest
  session player id and render handle, provider/locale, games completed, best
  score, total score, and last completed date with shared loading/error/empty
  state widgets.
- All-time leaderboard presentation baseline complete: `LeaderboardCubit` and
  `/leaderboard` route load the default all-time/bestScore list and render
  rank, handle, games completed, total score, and best score with shared
  loading/error/empty widgets. Guest ready and profile summary screens link to
  the leaderboard.
- Daily/weekly period selector complete: `LeaderboardCubit.changePeriod` and a
  Material `SegmentedButton` switch the list between all-time, daily, and
  weekly. The selector is disabled while loading; the subtitle and empty-state
  message adapt to the selected period. Live API smoke covered all 3 periods.
- Slice 7 is complete. Empty/error states across profile and leaderboard
  screens are already covered through shared widgets.

---

## Slice 10 — Home Landing UX ✅ done (2026-05-16)

Goal: Bootstrap design-system preview ekranini emekli edip uygulamanin
giris akisini (splash + ana ekran) urune dair bir tasarim diline tasimak.

- [x] Sand-pour `SplashScreen` (`/` route), `LexiLink` wordmark animasyonu.
- [x] Yeni `HomeScreen` (`/home`): top-right `EnergyBadge`, top-left
  profile/quests yuvarlak ikon butonlari, ortada swipeable kategori
  `PageView` carousel, alta `Start` butonu.
- [x] `ScrollConfiguration` + `_DragScrollBehavior` ile Flutter web'de
  PageView mouse drag aktif.
- [x] Kare kart `LayoutBuilder` + `cardSize = maxWidth * 0.82`; logo
  `FittedBox` ile `cardSize * 0.72` genislikte.
- [x] `_categoryVisuals(name)` emoji + gradient map'i (Hayvanlar 🦊,
  Yemekler 🍜, Doğa 🌿, default 🎲).
- [x] `MultiBlocListener` ile session check + silent guest auto-register
  + categories/energy preload; success → start oyunu yonlendir.
- [x] 3 sahte kategori `POST /categories` ile seed edildi (gecici content).
- [x] Ortak `AppBackBar` widget'i; profile/quests/leaderboard ust basliklari
  ve geri donusleri bu pattern'e tasindi.
- [x] Profile avatar + stats karti polish.
- [x] Quests tile'lara `LinearProgressIndicator` + reward badge polish.
- [x] `/guest` route + `guest_entry_screen.dart` + `features/bootstrap/`
  silindi; `app_test.dart` SplashScreen render kontrolune cevrildi.

Acceptance:

- App acilisi splash → home; manuel sign-in adimi gorulmuyor (silent
  guest auto-register).
- Carousel mouse/touch ile swipe edilebiliyor; aktif kategori dot ile
  belirtiliyor.
- Start butonu seçili kategori ile oyun olusturuyor (`/games/:id`).
- Profile/Quests/Leaderboard ekranlarinda ust soldaki geri butonu `/home`'a
  donduruyor.

Verification:

- `flutter analyze` 0 issue (1 non-blocking info).
- `flutter test` 45/45.
- Chrome smoke `localhost:5173`: splash + carousel swipe + side icon
  navigation + AppBackBar manuel dogrulandi.

Non-goals (Slice 11+):

- Kategori icin gercek `imageUrl` (backend Domain + DbUp migration).
- Splash → home arasi animatli page transition.
- Game screen tasarim dili pass'i.
- `/categories` legacy route'unun silinmesi.

---

## Slice 11 — Game Screen Polish ✅ done (2026-05-17)

Goal: oyun ekranini Slice 10 ile gelen tasarim diline tasimak —
`AppBackBar`, gradient current hero card, start↔dots↔target progress rail,
2x3 outlink grid, secondary action hierarchy ve `showModalBottomSheet`
result paneli. Ayrica `GameRepository.getOutgoing(linkId)` legacy yolunu
emekli edip `getOptions(gameId)` (backend deterministik 6'li alt küme,
previousLinkId her zaman kilitli) tek seçenek kaynagi yap.

- [x] `GameRepository.getOutgoingLinks(linkId)` silindi; `getOptions(gameId)`
  cubit'in zaten kullandigi tek path.
- [x] `GameDetails` DTO `startLinkId` + `targetLinkId` parse ediyor (backend
  zaten donduruyordu, parse edilmiyordu).
- [x] `AppBackBar`'a opsiyonel `onBack` callback ve `trailing` widget
  parametreleri eklendi (geri uyumlu).
- [x] Game ekrani ust basligi `AppBackBar` + back → "Quit game?" confirm
  dialog → `abandon` API. Sag tarafta `PopupMenuButton` overflow menu
  (Reset progress).
- [x] Start anchor (sol) ─ step dots progress rail (orta) ─ Target anchor
  (sag). Dots `stepsTaken/maxSteps` orani kadar dolu (sandy gold), kalani
  primary'nin 0.18 alpha tonu.
- [x] Gradient current hero card (`primary` → `primaryPressed`,
  20px radius, soft shadow) + `FittedBox` ile uzun Turkish word safe.
- [x] Breadcrumb (`startWord › … › currentWord`, son 3 kelime, ellipsis).
- [x] Status chips: Steps `n/m`, Hints `n`, Score `n` (Score yalnizca
  `game.score != null` iken).
- [x] 2x3 `GridView` outlink choices. Tile state'leri: normal,
  hint-recommended (sandy gold ring, 2px border), previous-link
  (`option.id == previousLinkId` — start ise `game.startLinkId`, ≥2 ise
  `history[stepsTaken-2].linkId`, muted ton + `Icons.undo` rozet), disabled.
- [x] Hint / Undo secondary butonlari (her ikisi de kalan sayaci gosterir).
- [x] Result paneli `showModalBottomSheet`: outcome icon + title (success
  yesil / danger kirmizi / abandoned muted), subtitle, summary stats
  (Score, Steps, Hints used), full path satiri, "Back to home" CTA.
  `BlocListener.listenWhen` ile yalnizca `isFinished` gecisinde acilir;
  `_resultShown` flag ile re-build sirasinda tekrar acilmaz.
- [x] Eskimiş `widgets/link_tile.dart` ve `widgets/game_info_card.dart`
  silindi; `widgets/` klasoru de bos kaldigi icin kaldirildi.
- [x] Test: `game_repository_test.dart` `getOptions` cagrisini test eder;
  `game_details_test.dart` fixture `startLinkId`/`targetLinkId` icerir.

Acceptance:

- `make step` akisi sorunsuz (live API smoke: `kış sporu` start, 6
  deterministik option dondu).
- Result state'ler degisik ekran yerine ayni AppScreen uzerinde
  bottom-sheet olarak acilir.
- `flutter analyze` 0 yeni issue (yalnizca Slice 10'dan kalan splash
  `prefer_int_literals` info kalir).
- Mevcut test seti gecmeye devam eder.

Verification:

- `flutter analyze` 0 yeni issue.
- `flutter test` 45/45.
- Live API smoke: `POST /games`, `POST /games/{id}/start`,
  `GET /games/{id}/options` 6 deterministik secenek dondu (Spor:
  `kış sporu` → spor, hokey, snowboard, kar, buz, buz pateni).

Non-goals:

- Energy decrement animasyonu (sonraki slice icin).
- Real-time multiplayer / score broadcast.
- Path history persistence (devam eden oyun browser refresh sonrasi
  restore edilmiyor; out-of-scope).
- "Play again" tek tikla yeni oyun (sheet'te yok; CTA `/home`'a yonlendiriyor).
- Sign-out / Reset session UI (token clear flow icin slice 12+ adayi).

---

## Slice 9 — Quests on frontend ✅ done (2026-05-15)

Goal: backend Quests modulunu (`GET /quests/me`, `POST /quests/{id}/claim`)
UI tarafinda kullanilabilir hale getirmek.

- [x] `PlayerQuest` DTO + `QuestState` enum.
- [x] `QuestRepository.getMe()` + `claim(id)`.
- [x] `QuestsCubit` (load + claim; per-tile claimingId; claim message).
- [x] `/quests` route + `QuestsScreen` (state-ordered tile list).
- [x] Claim button on ReadyToClaim quests + busy/disable handling.
- [x] SnackBar reward queued mesaji.
- [x] Guest ready ekraninda "View quests" girisi.
- [x] Repository + cubit unit testleri.

Acceptance:

- `/quests/me` listesi yuklenir; ReadyToClaim quest'ler en uste sirali gelir.
- Claim aksiyonu basarili olunca SnackBar reward queued mesaji gosterir
  ve liste reload olur.
- 401/404 kullaniciya backend mesaji ile ulasir.

Verification:

- `flutter analyze` passed (0 issue).
- `flutter test` passed 46/46.
- `flutter build web --dart-define=LEXILINK_API_BASE_URL=http://127.0.0.1:5099`
  passed.
- Live API smoke: `GET /quests/me` (yeni guest) `[]`,
  `POST /quests/{fake}/claim` 404 ProblemDetails, bearer-less claim 401.

Non-goals:

- Quest progress live tick animasyonu.
- Daily reset countdown UI.
- Reward arrival icin aktif energy polling (badge sonraki ekran
  ziyaretinde guncellenir).
- Push notification.

---

## Deferred

- Apple/Google native sign-in UI: backend provider verifier credential'lari ve
  mobile app registration netlesince.
- Push notifications: gercek retention/product ihtiyaci olusunca.
- Offline gameplay/cache: ilk online MVP sonrasinda.
- Advanced animations/game juice: Game Screen MVP stabil olduktan sonra.

---

## Non-Actions For Now

- Unity/Godot gibi ayri oyun motoruna gecmek.
- Her backend module icin frontend'de yapay domain layer acmak.
- UI tamamlanmadan kompleks state/event sourcing modeli kurmak.
- Web ve mobile icin bastan iki ayri codebase yaratmak.
