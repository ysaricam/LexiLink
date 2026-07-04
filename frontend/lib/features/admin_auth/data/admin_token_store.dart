import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Persistent storage for the admin JWT, kept separate from the player
/// session token under its own SharedPreferences key
/// ([_adminAccessTokenKey]). Signing out of the admin session does
/// not affect the player bearer and vice versa — admins and players
/// have orthogonal identities even when one human happens to wear
/// both hats during development.
class SharedPreferencesAdminTokenStore implements TokenStore {
  SharedPreferencesAdminTokenStore._({
    required SharedPreferencesAsync preferences,
    required String? cachedAccessToken,
  }) : _preferences = preferences,
       _accessToken = cachedAccessToken;

  static const _adminAccessTokenKey = 'lexilink.admin.accessToken';

  static Future<SharedPreferencesAdminTokenStore> create() async {
    final preferences = SharedPreferencesAsync();
    final cachedAccessToken = await preferences.getString(_adminAccessTokenKey);

    return SharedPreferencesAdminTokenStore._(
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
    final accessToken = await _preferences.getString(_adminAccessTokenKey);
    _accessToken = accessToken;
    return accessToken;
  }

  @override
  Future<void> saveAccessToken(String token) async {
    await _preferences.setString(_adminAccessTokenKey, token);
    _accessToken = token;
  }

  /// Admin sessions don't carry a player id — admin operations are
  /// authorized by the admin role claim on the JWT, not by mirroring a
  /// player identity. These methods exist solely to satisfy the
  /// [TokenStore] contract.
  @override
  Future<String?> readPlayerId() async => null;

  @override
  Future<void> savePlayerId(String playerId) async {}

  @override
  Future<AuthSessionMode?> readSessionMode() async => null;

  @override
  Future<void> saveSessionMode(AuthSessionMode mode) async {}

  @override
  Future<void> clear() async {
    await _preferences.remove(_adminAccessTokenKey);
    _accessToken = null;
  }
}
