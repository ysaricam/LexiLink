import 'package:shared_preferences/shared_preferences.dart';

abstract interface class TokenStore {
  String? get accessToken;

  Future<String?> readAccessToken();

  Future<void> saveAccessToken(String token);

  Future<void> clear();
}

class InMemoryTokenStore implements TokenStore {
  String? _accessToken;

  @override
  String? get accessToken => _accessToken;

  @override
  Future<String?> readAccessToken() async => _accessToken;

  @override
  Future<void> saveAccessToken(String token) async {
    _accessToken = token;
  }

  @override
  Future<void> clear() async {
    _accessToken = null;
  }
}

class SharedPreferencesTokenStore implements TokenStore {
  SharedPreferencesTokenStore._({
    required SharedPreferencesAsync preferences,
    required String? cachedAccessToken,
  }) : _preferences = preferences,
       _accessToken = cachedAccessToken;

  static const _accessTokenKey = 'lexilink.accessToken';

  static Future<SharedPreferencesTokenStore> create() async {
    final preferences = SharedPreferencesAsync();
    final cachedAccessToken = await preferences.getString(_accessTokenKey);

    return SharedPreferencesTokenStore._(
      preferences: preferences,
      cachedAccessToken: cachedAccessToken,
    );
  }

  final SharedPreferencesAsync _preferences;
  String? _accessToken;

  @override
  String? get accessToken => _accessToken;

  @override
  Future<String?> readAccessToken() async {
    final accessToken = await _preferences.getString(_accessTokenKey);
    _accessToken = accessToken;
    return accessToken;
  }

  @override
  Future<void> saveAccessToken(String token) async {
    await _preferences.setString(_accessTokenKey, token);
    _accessToken = token;
  }

  @override
  Future<void> clear() async {
    await _preferences.remove(_accessTokenKey);
    _accessToken = null;
  }
}
