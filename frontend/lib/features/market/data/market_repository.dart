import 'package:lexilink_app/features/market/data/market_models.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class MarketRepository {
  const MarketRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<MarketCategory>> fetchCategories() async {
    final raw = await _apiClient.getJsonList('/market/categories');
    return raw
        .cast<Map<String, dynamic>>()
        .map(MarketCategory.fromJson)
        .toList(growable: false);
  }

  Future<MarketItem> fetchItem(String id) async {
    final raw = await _apiClient.getJson('/market/items/$id');
    return MarketItem.fromJson(raw);
  }

  Future<List<MarketOrder>> fetchMyOrders({
    int limit = 50,
    int offset = 0,
  }) async {
    final raw = await _apiClient.getJsonList(
      '/market/orders/me',
      queryParameters: {'limit': '$limit', 'offset': '$offset'},
    );
    return raw
        .cast<Map<String, dynamic>>()
        .map(MarketOrder.fromJson)
        .toList(growable: false);
  }

  Future<void> buy(String itemId) async {
    await _apiClient.postJson(
      '/market/items/$itemId/buy',
      body: {
        'idempotencyKey':
            'market-${DateTime.now().microsecondsSinceEpoch}-$itemId',
      },
    );
  }
}
