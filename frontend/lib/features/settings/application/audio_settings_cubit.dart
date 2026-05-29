import 'package:bloc/bloc.dart';
import 'package:lexilink_app/features/settings/data/audio_preferences_repository.dart';
import 'package:lexilink_app/features/settings/data/audio_settings.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';

/// Owns the live [AudioSettings], persists changes through the repository, and
/// pushes them into the [AudioService]. The cubit is the single writer: the
/// service stays free of any bloc dependency and only receives applied values.
class AudioSettingsCubit extends Cubit<AudioSettings> {
  AudioSettingsCubit({
    required AudioService audioService,
    required AudioPreferencesRepository repository,
  }) : _audioService = audioService,
       _repository = repository,
       super(AudioSettings.defaults);

  final AudioService _audioService;
  final AudioPreferencesRepository _repository;

  /// Loads persisted settings and applies them to the audio service. Call once
  /// at startup before any playback begins.
  Future<void> load() async {
    final settings = await _repository.load();
    emit(settings);
    await _apply(settings);
  }

  Future<void> setMusicEnabled({required bool enabled}) =>
      _update(state.copyWith(musicEnabled: enabled));

  Future<void> setSfxEnabled({required bool enabled}) =>
      _update(state.copyWith(sfxEnabled: enabled));

  Future<void> setMusicVolume(double volume) =>
      _update(state.copyWith(musicVolume: volume.clamp(0.0, 1.0)));

  Future<void> setSfxVolume(double volume) =>
      _update(state.copyWith(sfxVolume: volume.clamp(0.0, 1.0)));

  Future<void> _update(AudioSettings settings) async {
    if (settings == state) return;
    emit(settings);
    await _repository.save(settings);
    await _apply(settings);
  }

  Future<void> _apply(AudioSettings settings) {
    return _audioService.applySettings(
      musicEnabled: settings.musicEnabled,
      sfxEnabled: settings.sfxEnabled,
      musicVolume: settings.musicVolume,
      sfxVolume: settings.sfxVolume,
    );
  }
}
