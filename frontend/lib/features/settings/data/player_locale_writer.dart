import 'package:http/http.dart' as http;
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

/// Persists the player's chosen locale to the backend (`Player.Locale`) so
/// Phase 2 content filtering can serve language-appropriate puzzles. Best
/// effort: the UI language already applies client-side, so a failed or
/// skipped write (e.g. no player signed in yet) is never surfaced.
// ignore: one_member_abstracts
abstract interface class PlayerLocaleWriter {
  Future<void> updateLocale(String backendLocale);
}

/// No-op for tests and for environments where a backend write isn't wanted.
class NoopPlayerLocaleWriter implements PlayerLocaleWriter {
  const NoopPlayerLocaleWriter();

  @override
  Future<void> updateLocale(String backendLocale) async {}
}

/// Calls `PATCH /players/{id}/profile`. It first GETs the player so the
/// existing avatar is preserved (the profile endpoint overwrites both fields),
/// and skips the write when the stored locale already matches.
class ApiPlayerLocaleWriter implements PlayerLocaleWriter {
  ApiPlayerLocaleWriter({ApiConfig? config}) : _config = config;

  final ApiConfig? _config;

  @override
  Future<void> updateLocale(String backendLocale) async {
    final tokenStore = await SharedPreferencesTokenStore.create();
    final playerId = await tokenStore.readPlayerId();
    if (playerId == null || playerId.isEmpty) return;

    final client = http.Client();
    try {
      final apiClient = ApiClient(
        config: _config ?? ApiConfig.local(),
        httpClient: client,
        tokenStore: tokenStore,
      );

      final player = await apiClient.getJson('/players/$playerId');
      if (player['locale'] == backendLocale) return;

      await apiClient.patchJson(
        '/players/$playerId/profile',
        body: <String, dynamic>{
          'avatarUrl': player['avatarUrl'],
          'locale': backendLocale,
        },
      );
    } finally {
      client.close();
    }
  }
}
