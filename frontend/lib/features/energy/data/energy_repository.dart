import 'package:lexilink_app/features/energy/data/player_energy.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class EnergyRepository {
  const EnergyRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<PlayerEnergy> getMe() async {
    final response = await _apiClient.getJson('/energy/me');
    return PlayerEnergy.fromJson(response);
  }
}
