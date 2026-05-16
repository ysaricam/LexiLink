import 'package:lexilink_app/shared/api/api_client.dart';

class GuestPlayerRepository {
  const GuestPlayerRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<String> registerGuest({
    required String deviceId,
    required String displayName,
    required String locale,
  }) async {
    final response = await _apiClient.postJson(
      '/players/guest',
      body: {
        'deviceId': deviceId,
        'displayName': displayName,
        'locale': locale,
      },
    );

    final id = response['id'];
    if (id is String && id.isNotEmpty) {
      return id;
    }

    throw StateError('Guest registration did not return a player id.');
  }
}
