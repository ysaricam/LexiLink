import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/features/settings/application/audio_settings_cubit.dart';
import 'package:lexilink_app/features/settings/data/audio_preferences_repository.dart';
import 'package:lexilink_app/features/settings/presentation/settings_screen.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';

/// No-op service so the widget test never touches a real player.
class _SilentAudioService extends AudioService {
  @override
  Future<void> playEffect(SoundEffect effect) async {}

  @override
  Future<void> applySettings({
    required bool musicEnabled,
    required bool sfxEnabled,
    required double musicVolume,
    required double sfxVolume,
  }) async {}
}

void main() {
  Future<void> pumpScreen(WidgetTester tester, AudioSettingsCubit cubit) {
    return tester.pumpWidget(
      MaterialApp(
        home: RepositoryProvider<AudioService>.value(
          value: _SilentAudioService(),
          child: BlocProvider<AudioSettingsCubit>.value(
            value: cubit,
            child: const SettingsScreen(),
          ),
        ),
      ),
    );
  }

  testWidgets('renders both channel switches and volume sliders', (
    tester,
  ) async {
    final cubit = AudioSettingsCubit(
      audioService: _SilentAudioService(),
      repository: InMemoryAudioPreferencesRepository(),
    );

    await pumpScreen(tester, cubit);

    expect(find.byType(SwitchListTile), findsNWidgets(2));
    expect(find.byType(Slider), findsNWidgets(2));
    expect(find.text('Music'), findsOneWidget);
    expect(find.text('Sound effects'), findsOneWidget);
  });

  testWidgets('toggling the music switch updates the cubit', (tester) async {
    final cubit = AudioSettingsCubit(
      audioService: _SilentAudioService(),
      repository: InMemoryAudioPreferencesRepository(),
    );

    await pumpScreen(tester, cubit);

    await tester.tap(find.text('Music'));
    await tester.pump();

    expect(cubit.state.musicEnabled, isFalse);
  });
}
