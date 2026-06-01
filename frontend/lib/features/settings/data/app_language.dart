import 'package:flutter/widgets.dart';

/// The five launch languages. Each carries the Flutter/ARB language code, the
/// region-qualified `xx-XX` form the backend `Player.Locale` expects, and the
/// endonym shown in the language picker (a language is always listed in its
/// own language, so these never need translation).
enum AppLanguage {
  turkish('tr', 'tr-TR', 'Türkçe'),
  english('en', 'en-US', 'English'),
  german('de', 'de-DE', 'Deutsch'),
  french('fr', 'fr-FR', 'Français'),
  spanish('es', 'es-ES', 'Español');

  const AppLanguage(this.code, this.backendLocale, this.nativeName);

  /// Flutter/ARB language code (e.g. `tr`).
  final String code;

  /// Region-qualified locale stored in `Player.Locale` (e.g. `tr-TR`),
  /// matching the backend rule (`^[a-z]{2}-[A-Z]{2}$`).
  final String backendLocale;

  /// Endonym for the picker (e.g. `Türkçe`).
  final String nativeName;

  Locale get locale => Locale(code);

  /// The fallback when the device language isn't one of the five.
  static const AppLanguage fallback = AppLanguage.english;

  /// Resolves by ARB language code (region-insensitive), or null if unknown.
  static AppLanguage? fromCode(String? code) {
    for (final language in AppLanguage.values) {
      if (language.code == code) return language;
    }
    return null;
  }
}
