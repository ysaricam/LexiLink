import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/features/settings/application/audio_settings_cubit.dart';
import 'package:lexilink_app/features/settings/application/color_palette_cubit.dart';
import 'package:lexilink_app/features/settings/application/locale_cubit.dart';
import 'package:lexilink_app/features/settings/data/audio_preferences_repository.dart';
import 'package:lexilink_app/features/settings/data/color_palette_preferences_repository.dart';
import 'package:lexilink_app/features/settings/data/locale_preferences_repository.dart';
import 'package:lexilink_app/features/settings/data/player_locale_writer.dart';
import 'package:lexilink_app/features/settings/presentation/settings_screen.dart';
import 'package:lexilink_app/l10n/app_localizations.dart';
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
        localizationsDelegates: AppLocalizations.localizationsDelegates,
        supportedLocales: AppLocalizations.supportedLocales,
        home: RepositoryProvider<AudioService>.value(
          value: _SilentAudioService(),
          child: MultiBlocProvider(
            providers: [
              BlocProvider<AudioSettingsCubit>.value(value: cubit),
              BlocProvider<ColorPaletteCubit>(
                create: (_) => ColorPaletteCubit(
                  repository: InMemoryColorPalettePreferencesRepository(),
                ),
              ),
              BlocProvider<LocaleCubit>(
                create: (_) => LocaleCubit(
                  repository: InMemoryLocalePreferencesRepository(),
                  localeWriter: const NoopPlayerLocaleWriter(),
                ),
              ),
            ],
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

    expect(find.byType(DropdownButtonHideUnderline), findsNWidgets(2));
    expect(find.byType(Switch), findsNWidgets(2));
    expect(find.byType(Slider), findsNWidgets(2));
    expect(find.text('Color palette'), findsOneWidget);
    expect(find.text('Classic'), findsOneWidget);
    expect(find.text('Music'), findsOneWidget);
    expect(find.text('Sound effects'), findsOneWidget);
    expect(find.text('Privacy and data'), findsOneWidget);
    expect(find.text('Privacy policy'), findsOneWidget);
    expect(find.text('Account deletion information'), findsOneWidget);
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
