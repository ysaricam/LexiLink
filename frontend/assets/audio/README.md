# Audio assets

These files back the `SoundEffect` and `MusicTrack` catalogs in
`lib/shared/audio/audio_service.dart`. The current files are production
candidate WAV assets copied from `docs/ses/`.

## Replacing with real sounds

Drop a real file at the **same path with the same name** and it plays with no
code change. Keep the filenames; the enum entries map to them by path.

- Prefer `.wav` (or `.mp3`/`.ogg`) - if you change the extension, update the
  matching enum entry's `asset` string in `audio_service.dart`.
- Keep SFX short (≤ ~1s). Keep music seamlessly loopable (it plays with
  `ReleaseMode.loop`).
- After replacing files run `flutter pub get` is **not** required (paths are
  already registered in `pubspec.yaml` under `assets/audio/`), but do a clean
  rebuild so the browser/web cache picks up new bytes.

## Files

Current format: WAV PCM, 16-bit stereo, 48 kHz.

### `sfx/` — one-shot effects (`SoundEffect`)

| File | SoundEffect | Played when |
| --- | --- | --- |
| `button_tap.wav` | `buttonTap` | Home Start + side-nav taps; settings SFX toggle preview |
| `step.wav` | `step` | A move that did not finish the game |
| `hint.wav` | `hint` | Hint used |
| `undo.wav` | `undo` | Undo used |
| `reset.wav` | `reset` | Reset used |
| `win.wav` | `win` | Game reaches `Completed` |
| `lose.wav` | `lose` | Game reaches `Failed` |
| `quest_claim.wav` | `questClaim` | Quest reward claimed |
| `purchase.wav` | `purchase` | Market buy / Payments grant succeeds |
| `error.wav` | `error` | A failed action / failed start / failed buy |

### `music/` — looping background tracks (`MusicTrack`)

| File | MusicTrack | Played on |
| --- | --- | --- |
| `menu.wav` | `menu` | Home and other player screens |
| `game.wav` | `game` | In-game (`/games/*`) |
