import 'package:lexilink_app/features/reset/data/player_reset.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class ResetRepository {
  const ResetRepository({required ApiClient apiClient}) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<PlayerReset> getMe() async {
    final response = await _apiClient.getJson('/reset/me');
    return PlayerReset.fromJson(response);
  }
}
