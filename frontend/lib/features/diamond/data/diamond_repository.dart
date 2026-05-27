import 'package:lexilink_app/features/diamond/data/player_diamond.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class DiamondRepository {
  const DiamondRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<PlayerDiamond> getMe() async {
    final response = await _apiClient.getJson('/diamond/me');
    return PlayerDiamond.fromJson(response);
  }
}
