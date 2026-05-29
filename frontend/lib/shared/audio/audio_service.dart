import 'package:audioplayers/audioplayers.dart';
import 'package:flutter/foundation.dart';

/// The catalog of one-shot sound effects. Each entry carries the asset
/// path (relative to the `assets/` prefix audioplayers adds). Real sounds
/// are swapped in later by replacing the files under `assets/audio/sfx/`;
/// this enum and its call sites do not change.
enum SoundEffect {
  buttonTap('audio/sfx/button_tap.wav'),
  step('audio/sfx/step.wav'),
  hint('audio/sfx/hint.wav'),
  undo('audio/sfx/undo.wav'),
  reset('audio/sfx/reset.wav'),
  win('audio/sfx/win.wav'),
  lose('audio/sfx/lose.wav'),
  questClaim('audio/sfx/quest_claim.wav'),
  purchase('audio/sfx/purchase.wav'),
  error('audio/sfx/error.wav');

  const SoundEffect(this.asset);

  final String asset;
}

/// The catalog of looping background-music tracks.
enum MusicTrack {
  menu('audio/music/menu.wav'),
  game('audio/music/game.wav');

  const MusicTrack(this.asset);

  final String asset;
}

/// Builds an [AudioPlayer]. Injectable so tests can substitute a fake.
typedef AudioPlayerFactory = AudioPlayer Function();

/// A single, app-wide audio facade. It owns one looping music player and a
/// small round-robin pool of players for overlapping sound effects, and it
/// honours the current music/SFX enabled flags and volumes (fed by the
/// settings cubit in a later slice).
///
/// Every playback call is best-effort: a missing asset or a blocked-autoplay
/// browser must never throw into the UI. Failures are swallowed.
class AudioService {
  AudioService({
    AudioPlayerFactory? playerFactory,
    int sfxPoolSize = 4,
  }) : _playerFactory = playerFactory ?? AudioPlayer.new,
       _sfxPoolSize = sfxPoolSize;

  final AudioPlayerFactory _playerFactory;
  final int _sfxPoolSize;

  final List<AudioPlayer> _sfxPool = <AudioPlayer>[];
  int _sfxCursor = 0;

  AudioPlayer? _musicPlayer;

  /// The track the service should be playing while music is enabled. Kept so
  /// re-enabling music or returning from background can resume the right loop.
  MusicTrack? _desiredTrack;

  bool _musicEnabled = true;
  bool _sfxEnabled = true;
  double _musicVolume = 0.5;
  double _sfxVolume = 0.8;

  bool get musicEnabled => _musicEnabled;

  bool get sfxEnabled => _sfxEnabled;

  double get musicVolume => _musicVolume;

  double get sfxVolume => _sfxVolume;

  MusicTrack? get currentTrack => _desiredTrack;

  /// Plays a one-shot sound effect if SFX are enabled and audible.
  Future<void> playEffect(SoundEffect effect) async {
    if (!_sfxEnabled || _sfxVolume <= 0) return;
    try {
      final player = _nextSfxPlayer();
      await player.stop();
      await _evictWebCache(effect.asset);
      await player.play(AssetSource(effect.asset), volume: _sfxVolume);
    } on Object catch (_) {
      // Best-effort: never let audio break the UX. Catches Error too (web
      // throws UnsupportedError from audioplayers' dart:io cache recheck).
    }
  }

  /// Starts (or switches to) a looping background track. Remembers the track
  /// even when music is disabled so it can resume once re-enabled. Requesting
  /// the track that is already active is a no-op, so repeated route
  /// notifications never restart the loop.
  Future<void> playMusic(MusicTrack track) async {
    if (track == _desiredTrack) return;
    _desiredTrack = track;
    if (!_musicEnabled || _musicVolume <= 0) return;
    await _startDesiredTrack();
  }

  /// Stops background music and clears the desired track.
  Future<void> stopMusic() async {
    _desiredTrack = null;
    try {
      await _musicPlayer?.stop();
    } on Object catch (_) {
      // Best-effort.
    }
  }

  /// Pauses background music without forgetting the desired track
  /// (used by the app-lifecycle listener when going to background).
  Future<void> pauseMusic() async {
    try {
      await _musicPlayer?.pause();
    } on Object catch (_) {
      // Best-effort.
    }
  }

  /// Resumes the desired track if music is enabled (foregrounding).
  Future<void> resumeMusic() async {
    if (!_musicEnabled || _musicVolume <= 0 || _desiredTrack == null) return;
    await _startDesiredTrack();
  }

  /// Applies new preferences. Adjusts live volume, stops music when disabled,
  /// and resumes the desired track when re-enabled.
  Future<void> applySettings({
    required bool musicEnabled,
    required bool sfxEnabled,
    required double musicVolume,
    required double sfxVolume,
  }) async {
    final wasMusicAudible = _musicEnabled && _musicVolume > 0;
    _musicEnabled = musicEnabled;
    _sfxEnabled = sfxEnabled;
    _musicVolume = musicVolume.clamp(0.0, 1.0);
    _sfxVolume = sfxVolume.clamp(0.0, 1.0);

    final isMusicAudible = _musicEnabled && _musicVolume > 0;
    try {
      if (!isMusicAudible) {
        await _musicPlayer?.pause();
      } else {
        await _musicPlayer?.setVolume(_musicVolume);
        if (!wasMusicAudible && _desiredTrack != null) {
          await _startDesiredTrack();
        }
      }
    } on Object catch (_) {
      // Best-effort.
    }
  }

  /// Releases every underlying player. Call when the app shuts down.
  Future<void> dispose() async {
    for (final player in _sfxPool) {
      await player.dispose();
    }
    _sfxPool.clear();
    await _musicPlayer?.dispose();
    _musicPlayer = null;
  }

  Future<void> _startDesiredTrack() async {
    final track = _desiredTrack;
    if (track == null) return;
    try {
      final player = _musicPlayer ??= _playerFactory();
      await player.setReleaseMode(ReleaseMode.loop);
      await _evictWebCache(track.asset);
      await player.play(AssetSource(track.asset), volume: _musicVolume);
    } on Object catch (_) {
      // Best-effort.
    }
  }

  /// Works around an audioplayers-on-web bug: its asset cache rechecks the
  /// file with a `dart:io` call on the second+ play of the same asset, which
  /// throws `UnsupportedError` in the browser. Evicting the cache entry first
  /// forces a fresh (browser-cached) fetch and skips that path. No-op and
  /// unnecessary on native platforms.
  Future<void> _evictWebCache(String asset) async {
    if (!kIsWeb) return;
    await AudioCache.instance.clear(asset);
  }

  AudioPlayer _nextSfxPlayer() {
    if (_sfxPool.length < _sfxPoolSize) {
      final player = _playerFactory();
      _sfxPool.add(player);
      return player;
    }
    final player = _sfxPool[_sfxCursor];
    _sfxCursor = (_sfxCursor + 1) % _sfxPool.length;
    return player;
  }
}
