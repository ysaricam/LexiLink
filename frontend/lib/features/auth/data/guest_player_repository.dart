import 'package:lexilink_app/features/auth/data/social_identity.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class GuestSession {
  const GuestSession({required this.playerId, required this.accessToken});

  final String playerId;
  final String accessToken;
}

class GuestPlayerRepository {
  const GuestPlayerRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  /// Registers (or rehydrates) a guest player and exchanges the
  /// returned identity for a first-party JWT. In DevelopmentBearer
  /// mode the JWT is unused — the auth handler accepts any GUID — but
  /// keeping the round-trip uniform means ProductionJwt deployments
  /// work without a separate code path.
  Future<GuestSession> registerGuest({
    required String deviceId,
    required String displayName,
    required String locale,
  }) async {
    final registerResponse = await _apiClient.postJson(
      '/players/guest',
      body: {
        'deviceId': deviceId,
        'displayName': displayName,
        'locale': locale,
      },
    );

    final playerId = registerResponse['id'];
    if (playerId is! String || playerId.isEmpty) {
      throw StateError('Guest registration did not return a player id.');
    }

    final tokenResponse = await _apiClient.postJson(
      '/auth/token',
      body: {
        'provider': 'Guest',
        'externalId': deviceId,
        'externalToken': 'dev:Guest:$deviceId',
      },
    );

    final accessToken = tokenResponse['accessToken'];
    if (accessToken is! String || accessToken.isEmpty) {
      throw StateError('Token exchange did not return an access token.');
    }

    return GuestSession(playerId: playerId, accessToken: accessToken);
  }

  Future<GuestSession> exchangeSocialIdentity(SocialIdentity identity) async {
    final tokenResponse = await _apiClient.postJson(
      '/auth/token',
      body: {
        'provider': identity.provider.apiValue,
        'externalId': identity.externalId,
        'externalToken': identity.externalToken,
      },
    );

    final accessToken = tokenResponse['accessToken'];
    final playerId = tokenResponse['playerId'];
    if (accessToken is! String || accessToken.isEmpty) {
      throw StateError('Token exchange did not return an access token.');
    }
    if (playerId is! String || playerId.isEmpty) {
      throw StateError('Token exchange did not return a player id.');
    }

    return GuestSession(playerId: playerId, accessToken: accessToken);
  }
}
