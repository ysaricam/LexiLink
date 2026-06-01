import 'package:lexilink_app/features/settings/data/app_language.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Reads and writes the player's chosen [AppLanguage], device-local. Returns
/// `null` when the player has never picked a language, so the cubit can fall
/// back to the device locale on first launch. Same abstraction shape as
/// `AudioPreferencesRepository` — the backing store can change without
/// touching the cubit/UI.
abstract interface class LocalePreferencesRepository {
  Future<AppLanguage?> load();

  Future<void> save(AppLanguage language);
}

/// In-memory implementation for tests.
class InMemoryLocalePreferencesRepository
    implements LocalePreferencesRepository {
  InMemoryLocalePreferencesRepository([this._language]);

  AppLanguage? _language;

  @override
  Future<AppLanguage?> load() async => _language;

  @override
  Future<void> save(AppLanguage language) async {
    _language = language;
  }
}

/// SharedPreferences-backed implementation. A missing or unknown stored code
/// resolves to `null` (treated as "never chosen").
class SharedPreferencesLocalePreferencesRepository
    implements LocalePreferencesRepository {
  SharedPreferencesLocalePreferencesRepository({
    SharedPreferencesAsync? preferences,
  }) : _preferences = preferences ?? SharedPreferencesAsync();

  static const _languageCodeKey = 'lexilink.locale.languageCode';

  final SharedPreferencesAsync _preferences;

  @override
  Future<AppLanguage?> load() async {
    final code = await _preferences.getString(_languageCodeKey);
    return AppLanguage.fromCode(code);
  }

  @override
  Future<void> save(AppLanguage language) async {
    await _preferences.setString(_languageCodeKey, language.code);
  }
}
