import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/features/settings/application/audio_settings_cubit.dart';
import 'package:lexilink_app/features/settings/data/audio_preferences_repository.dart';
import 'package:lexilink_app/features/settings/data/audio_settings.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';

/// Captures the values pushed into the audio service without touching any
/// real player.
class _RecordingAudioService extends AudioService {
  final List<AudioSettings> applied = [];

  @override
  Future<void> applySettings({
    required bool musicEnabled,
    required bool sfxEnabled,
    required double musicVolume,
    required double sfxVolume,
  }) async {
    applied.add(
      AudioSettings(
        musicEnabled: musicEnabled,
        sfxEnabled: sfxEnabled,
        musicVolume: musicVolume,
        sfxVolume: sfxVolume,
      ),
    );
  }
}

void main() {
  late _RecordingAudioService audioService;

  AudioSettingsCubit buildCubit(AudioPreferencesRepository repository) {
    return AudioSettingsCubit(
      audioService: audioService,
      repository: repository,
    );
  }

  setUp(() {
    audioService = _RecordingAudioService();
  });

  test('load emits and applies the persisted settings', () async {
    const stored = AudioSettings(
      musicEnabled: false,
      sfxEnabled: false,
      musicVolume: 0.2,
      sfxVolume: 0.9,
    );
    final cubit = buildCubit(InMemoryAudioPreferencesRepository(stored));

    await cubit.load();

    expect(cubit.state, stored);
    expect(audioService.applied, [stored]);
  });

  test('toggling a channel persists and applies the change', () async {
    final repository = InMemoryAudioPreferencesRepository();
    final cubit = buildCubit(repository);

    await cubit.setMusicEnabled(enabled: false);

    expect(cubit.state.musicEnabled, isFalse);
    expect((await repository.load()).musicEnabled, isFalse);
    expect(audioService.applied.last.musicEnabled, isFalse);
  });

  test('volume setters clamp to the 0..1 range', () async {
    final cubit = buildCubit(InMemoryAudioPreferencesRepository());

    await cubit.setSfxVolume(1.7);
    expect(cubit.state.sfxVolume, 1.0);

    await cubit.setMusicVolume(-0.5);
    expect(cubit.state.musicVolume, 0.0);
  });

  test('a no-op change is not re-emitted or re-applied', () async {
    final cubit = buildCubit(InMemoryAudioPreferencesRepository());

    await cubit.setSfxEnabled(enabled: true); // already the default

    expect(audioService.applied, isEmpty);
  });
}
