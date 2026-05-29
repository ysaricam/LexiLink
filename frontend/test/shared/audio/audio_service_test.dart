import 'package:audioplayers/audioplayers.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';
import 'package:mocktail/mocktail.dart';

class _MockAudioPlayer extends Mock implements AudioPlayer {}

void main() {
  setUpAll(() {
    registerFallbackValue(AssetSource('audio/sfx/step.wav'));
    registerFallbackValue(ReleaseMode.loop);
  });

  late _MockAudioPlayer player;

  AudioService buildService() => AudioService(playerFactory: () => player);

  setUp(() {
    player = _MockAudioPlayer();
    when(() => player.stop()).thenAnswer((_) async {});
    when(() => player.pause()).thenAnswer((_) async {});
    when(() => player.dispose()).thenAnswer((_) async {});
    when(() => player.setVolume(any())).thenAnswer((_) async {});
    when(() => player.setReleaseMode(any())).thenAnswer((_) async {});
    when(
      () => player.play(any(), volume: any(named: 'volume')),
    ).thenAnswer((_) async {});
  });

  group('asset catalog', () {
    test('every sound effect maps to an sfx wav asset', () {
      for (final effect in SoundEffect.values) {
        expect(effect.asset, startsWith('audio/sfx/'));
        expect(effect.asset, endsWith('.wav'));
      }
    });

    test('every music track maps to a music wav asset', () {
      for (final track in MusicTrack.values) {
        expect(track.asset, startsWith('audio/music/'));
        expect(track.asset, endsWith('.wav'));
      }
    });
  });

  group('playEffect', () {
    test('plays the matching asset when SFX are enabled', () async {
      final service = buildService();

      await service.playEffect(SoundEffect.win);

      final captured = verify(
        () => player.play(captureAny(), volume: any(named: 'volume')),
      ).captured.single;
      expect(captured, isA<AssetSource>());
      expect((captured as AssetSource).path, SoundEffect.win.asset);
    });

    test('does nothing when SFX are disabled', () async {
      final service = buildService();
      await service.applySettings(
        musicEnabled: true,
        sfxEnabled: false,
        musicVolume: 0.5,
        sfxVolume: 0.8,
      );

      await service.playEffect(SoundEffect.win);

      verifyNever(() => player.play(any(), volume: any(named: 'volume')));
    });
  });

  group('playMusic', () {
    test('loops the requested track and remembers it', () async {
      final service = buildService();

      await service.playMusic(MusicTrack.menu);

      expect(service.currentTrack, MusicTrack.menu);
      verify(() => player.setReleaseMode(ReleaseMode.loop)).called(1);
      final captured = verify(
        () => player.play(captureAny(), volume: any(named: 'volume')),
      ).captured.single;
      expect((captured as AssetSource).path, MusicTrack.menu.asset);
    });

    test('requesting the active track again does not restart it', () async {
      final service = buildService();

      await service.playMusic(MusicTrack.menu);
      await service.playMusic(MusicTrack.menu);

      verify(
        () => player.play(any(), volume: any(named: 'volume')),
      ).called(1);
    });

    test('remembers the track but stays silent when music is disabled',
        () async {
      final service = buildService();
      await service.applySettings(
        musicEnabled: false,
        sfxEnabled: true,
        musicVolume: 0.5,
        sfxVolume: 0.8,
      );

      await service.playMusic(MusicTrack.game);

      expect(service.currentTrack, MusicTrack.game);
      verifyNever(() => player.play(any(), volume: any(named: 'volume')));
    });
  });
}
