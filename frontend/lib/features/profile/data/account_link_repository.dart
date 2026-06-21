import 'package:lexilink_app/features/auth/data/social_identity.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class AccountLinkRepository {
  const AccountLinkRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

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
