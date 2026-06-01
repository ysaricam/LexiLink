import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:lexilink_app/app/router/app_router.dart';
import 'package:lexilink_app/app/theme/app_theme.dart';
import 'package:lexilink_app/features/settings/application/audio_settings_cubit.dart';
import 'package:lexilink_app/features/settings/application/locale_cubit.dart';
import 'package:lexilink_app/features/settings/data/app_language.dart';
import 'package:lexilink_app/features/settings/data/audio_preferences_repository.dart';
import 'package:lexilink_app/features/settings/data/locale_preferences_repository.dart';
import 'package:lexilink_app/features/settings/data/player_locale_writer.dart';
import 'package:lexilink_app/l10n/app_localizations.dart';
import 'package:lexilink_app/shared/ads/ads_service.dart';
import 'package:lexilink_app/shared/audio/audio_music_orchestrator.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';

class LexiLinkApp extends StatelessWidget {
  const LexiLinkApp({
    required this.audioService,
    required this.adsService,
    this.audioPreferencesRepository,
    this.localePreferencesRepository,
    this.playerLocaleWriter,
    super.key,
  });

  /// App-wide audio facade. Provided above the router so every screen can
  /// reach the same instance via `context.read<AudioService>()` — audio is a
  /// genuinely global concern, like session and theme.
  final AudioService audioService;

  /// App-wide ads facade. Provided above the router like [audioService];
  /// mobile-only and web-safe (a no-op on unsupported platforms).
  final AdsService adsService;

  /// Overridable for tests; defaults to the device-local SharedPreferences
  /// store in production.
  final AudioPreferencesRepository? audioPreferencesRepository;

  /// Overridable for tests; defaults to the device-local SharedPreferences
  /// store in production.
  final LocalePreferencesRepository? localePreferencesRepository;

  /// Overridable for tests; defaults to the backend `Player.Locale` writer.
  final PlayerLocaleWriter? playerLocaleWriter;

  @override
  Widget build(BuildContext context) {
    return RepositoryProvider<AudioService>.value(
      value: audioService,
      child: RepositoryProvider<AdsService>.value(
        value: adsService,
        child: MultiBlocProvider(
          providers: [
            BlocProvider<AudioSettingsCubit>(
              create: (_) => AudioSettingsCubit(
                audioService: audioService,
                repository: audioPreferencesRepository ??
                    SharedPreferencesAudioPreferencesRepository(),
              )..load(),
            ),
            BlocProvider<LocaleCubit>(
              create: (_) => LocaleCubit(
                repository: localePreferencesRepository ??
                    SharedPreferencesLocalePreferencesRepository(),
                localeWriter: playerLocaleWriter ?? ApiPlayerLocaleWriter(),
              )..load(),
            ),
          ],
          child: AudioMusicOrchestrator(
            audioService: audioService,
            router: appRouter,
            child: BlocBuilder<LocaleCubit, AppLanguage>(
              builder: (context, language) => MaterialApp.router(
                onGenerateTitle: (context) => context.l10n.appTitle,
                localizationsDelegates:
                    AppLocalizations.localizationsDelegates,
                supportedLocales: AppLocalizations.supportedLocales,
                // LocaleCubit drives the active locale. The resolution
                // callback is a defensive fallback to English for any
                // unexpected unsupported value.
                locale: language.locale,
                localeListResolutionCallback: _resolveLocale,
                theme: AppTheme.light,
                darkTheme: AppTheme.dark,
                routerConfig: appRouter,
              ),
            ),
          ),
        ),
      ),
    );
  }

  /// Picks the first device locale whose language is supported, else falls
  /// back to English. Matching is by language code so any region variant of
  /// a supported language resolves (e.g. `de-AT` → `de`).
  static Locale _resolveLocale(
    List<Locale>? deviceLocales,
    Iterable<Locale> supportedLocales,
  ) {
    final supportedLanguages = supportedLocales
        .map((locale) => locale.languageCode)
        .toSet();
    for (final locale in deviceLocales ?? const <Locale>[]) {
      if (supportedLanguages.contains(locale.languageCode)) {
        return Locale(locale.languageCode);
      }
    }
    return const Locale('en');
  }
}
