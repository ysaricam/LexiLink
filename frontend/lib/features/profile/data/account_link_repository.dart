import 'package:lexilink_app/features/auth/data/social_identity.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class AccountLinkRepository {
  const AccountLinkRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<AppleContinueSession> continueWithApple({
    required SocialIdentity identity,
  }) async {
    final response = await _apiClient.postJson(
      '/auth/apple/continue',
      body: {
        'externalId': identity.externalId,
        'externalToken': identity.externalToken,
        'email': identity.email,
      },
    );

    final accessToken = response['accessToken'];
    final playerId = response['playerId'];
    final mode = response['mode'];
    if (accessToken is! String || accessToken.isEmpty) {
      throw StateError('Apple continue did not return an access token.');
    }
    if (playerId is! String || playerId.isEmpty) {
      throw StateError('Apple continue did not return a player id.');
    }
    if (mode is! String || mode.isEmpty) {
      throw StateError('Apple continue did not return a mode.');
    }

    return AppleContinueSession(
      playerId: playerId,
      accessToken: accessToken,
      mode: AppleContinueMode.fromApiValue(mode),
    );
  }

  Future<void> linkProvider({
    required String playerId,
    required SocialIdentity identity,
  }) async {
    await _apiClient.postJson(
      '/players/$playerId/auth-providers',
      body: {
        'provider': identity.provider.apiValue,
        'externalId': identity.externalId,
        'externalToken': identity.externalToken,
        'email': identity.email,
      },
    );
  }
}

class AppleContinueSession {
  const AppleContinueSession({
    required this.playerId,
    required this.accessToken,
    required this.mode,
  });

  final String playerId;
  final String accessToken;
  final AppleContinueMode mode;
}

enum AppleContinueMode {
  linkedCurrentGuest('LinkedCurrentGuest'),
  switchedToExistingApplePlayer('SwitchedToExistingApplePlayer');

  const AppleContinueMode(this.apiValue);

  final String apiValue;

  static AppleContinueMode fromApiValue(String value) {
    return AppleContinueMode.values.firstWhere(
      (mode) => mode.apiValue == value,
      orElse: () => throw StateError('Unknown Apple continue mode: $value'),
    );
  }
}
