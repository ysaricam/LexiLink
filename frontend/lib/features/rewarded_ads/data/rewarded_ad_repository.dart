import 'package:lexilink_app/features/rewarded_ads/data/rewarded_ad_status.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class RewardedAdRepository {
  const RewardedAdRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<RewardedAdStatus> getStatus() async {
    final response = await _apiClient.getJson('/ads/rewarded/status');
    return RewardedAdStatus.fromJson(response);
  }
}
