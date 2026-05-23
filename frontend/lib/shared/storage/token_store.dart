import 'package:shared_preferences/shared_preferences.dart';

/// Persists the player session: the JWT used as a bearer credential,
/// and the player's id (a separate value because in ProductionJwt
/// mode the JWT subject is not directly accessible without parsing
/// it client-side; in DevelopmentBearer mode the JWT happens to equal
/// the GUID, but consumers shouldn't rely on that).
abstract interface class TokenStore {
  String? get accessToken;

  Future<String?> readAccessToken();

  Future<void> saveAccessToken(String token);

  Future<String?> readPlayerId();

  Future<void> savePlayerId(String playerId);

  Future<void> clear();
}

class InMemoryTokenStore implements TokenStore {
  String? _accessToken;
  String? _playerId;

  @override
  String? get accessToken => _accessToken;

  @override
  Future<String?> readAccessToken() async => _accessToken;

  @override
  Future<void> saveAccessToken(String token) async {
    _accessToken = token;
  }

  @override
  Future<String?> readPlayerId() async => _playerId;

  @override
  Future<void> savePlayerId(String playerId) async {
    _playerId = playerId;
  }

  @override
  Future<void> clear() async {
    _accessToken = null;
    _playerId = null;
  }
}

class SharedPreferencesTokenStore implements TokenStore {
  SharedPreferencesTokenStore._({
    required SharedPreferencesAsync preferences,
    required String? cachedAccessToken,
  }) : _preferences = preferences,
       _accessToken = cachedAccessToken;

  static const _accessTokenKey = 'lexilink.accessToken';
  static const _playerIdKey = 'lexilink.playerId';

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
  Future<String?> readPlayerId() =>
      _preferences.getString(_playerIdKey);

  @override
  Future<void> savePlayerId(String playerId) =>
      _preferences.setString(_playerIdKey, playerId);

  @override
  Future<void> clear() async {
    await _preferences.remove(_accessTokenKey);
    await _preferences.remove(_playerIdKey);
    _accessToken = null;
  }
}
