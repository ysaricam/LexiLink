import 'package:lexilink_app/features/hint/data/player_hint.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class HintRepository {
  const HintRepository({required ApiClient apiClient}) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<PlayerHint> getMe() async {
    final response = await _apiClient.getJson('/hint/me');
    return PlayerHint.fromJson(response);
  }
}
