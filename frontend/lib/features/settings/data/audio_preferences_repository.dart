import 'package:lexilink_app/features/settings/data/audio_settings.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Reads and writes [AudioSettings]. The abstraction is deliberate: today it
/// is device-local (SharedPreferences); if a cross-device "sync my settings"
/// need ever appears, only the backing implementation changes — the cubit and
/// UI stay put.
abstract interface class AudioPreferencesRepository {
  Future<AudioSettings> load();

  Future<void> save(AudioSettings settings);
}

/// In-memory implementation for tests (and a safe non-persistent fallback).
class InMemoryAudioPreferencesRepository
    implements AudioPreferencesRepository {
  InMemoryAudioPreferencesRepository([AudioSettings? initial])
    : _settings = initial ?? AudioSettings.defaults;

  AudioSettings _settings;

  @override
  Future<AudioSettings> load() async => _settings;

  @override
  Future<void> save(AudioSettings settings) async {
    _settings = settings;
  }
}

/// SharedPreferences-backed implementation. Missing keys fall back to the
/// shipped defaults, so a fresh install behaves predictably.
class SharedPreferencesAudioPreferencesRepository
    implements AudioPreferencesRepository {
  SharedPreferencesAudioPreferencesRepository({
    SharedPreferencesAsync? preferences,
  }) : _preferences = preferences ?? SharedPreferencesAsync();

  static const _musicEnabledKey = 'lexilink.audio.musicEnabled';
  static const _sfxEnabledKey = 'lexilink.audio.sfxEnabled';
  static const _musicVolumeKey = 'lexilink.audio.musicVolume';
  static const _sfxVolumeKey = 'lexilink.audio.sfxVolume';

  final SharedPreferencesAsync _preferences;

  @override
  Future<AudioSettings> load() async {
    const defaults = AudioSettings.defaults;
    return AudioSettings(
      musicEnabled:
          await _preferences.getBool(_musicEnabledKey) ?? defaults.musicEnabled,
      sfxEnabled:
          await _preferences.getBool(_sfxEnabledKey) ?? defaults.sfxEnabled,
      musicVolume:
          await _preferences.getDouble(_musicVolumeKey) ?? defaults.musicVolume,
      sfxVolume:
          await _preferences.getDouble(_sfxVolumeKey) ?? defaults.sfxVolume,
    );
  }

  @override
  Future<void> save(AudioSettings settings) async {
    await _preferences.setBool(_musicEnabledKey, settings.musicEnabled);
    await _preferences.setBool(_sfxEnabledKey, settings.sfxEnabled);
    await _preferences.setDouble(_musicVolumeKey, settings.musicVolume);
    await _preferences.setDouble(_sfxVolumeKey, settings.sfxVolume);
  }
}
