import 'dart:ui';

import 'package:bloc/bloc.dart';
import 'package:lexilink_app/features/settings/data/app_language.dart';
import 'package:lexilink_app/features/settings/data/locale_preferences_repository.dart';
import 'package:lexilink_app/features/settings/data/player_locale_writer.dart';

/// Resolves the device's current language code; injectable for tests.
typedef DeviceLanguageResolver = String Function();

String _platformLanguageCode() =>
    PlatformDispatcher.instance.locale.languageCode;

/// Owns the active [AppLanguage] that drives `MaterialApp.router`'s locale.
/// On first launch it follows the device language (English fallback); once the
/// player picks a language it persists device-local and best-effort writes
/// `Player.Locale` to the backend. Single writer, like `AudioSettingsCubit`.
class LocaleCubit extends Cubit<AppLanguage> {
  LocaleCubit({
    required LocalePreferencesRepository repository,
    required PlayerLocaleWriter localeWriter,
    DeviceLanguageResolver deviceLanguageResolver = _platformLanguageCode,
  }) : _repository = repository,
       _localeWriter = localeWriter,
       _deviceLanguageResolver = deviceLanguageResolver,
       super(AppLanguage.fallback);

  final LocalePreferencesRepository _repository;
  final PlayerLocaleWriter _localeWriter;
  final DeviceLanguageResolver _deviceLanguageResolver;

  /// Loads the stored language, or resolves the device language with an
  /// English fallback when nothing is stored. Robust to a failing store.
  Future<void> load() async {
    AppLanguage? stored;
    try {
      stored = await _repository.load();
    } on Object catch (_) {
      stored = null;
    }
    emit(stored ?? _deviceLanguage());
  }

  /// Switches language: applies live (state drives the router locale),
  /// persists device-local, and best-effort writes `Player.Locale`.
  Future<void> setLanguage(AppLanguage language) async {
    if (language == state) return;
    emit(language);
    try {
      await _repository.save(language);
    } on Object catch (_) {
      // Best-effort persistence; the live language still applied.
    }
    try {
      await _localeWriter.updateLocale(language.backendLocale);
    } on Object catch (_) {
      // Best-effort backend sync; consumed by Phase 2 content filtering.
    }
  }

  AppLanguage _deviceLanguage() =>
      AppLanguage.fromCode(_deviceLanguageResolver()) ?? AppLanguage.fallback;
}
