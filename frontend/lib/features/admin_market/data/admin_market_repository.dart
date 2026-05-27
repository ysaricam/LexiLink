import 'package:lexilink_app/features/market/data/market_models.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class AdminMarketRepository {
  const AdminMarketRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<MarketCategory>> fetchCategories() async {
    final raw = await _apiClient.getJsonList('/admin/market/categories');
    return raw
        .cast<Map<String, dynamic>>()
        .map((json) {
          return MarketCategory(
            id: json['id'] as String,
            name: json['name'] as String,
            sortOrder: json['sortOrder'] as int,
            icon: json['icon'] as String?,
            isActive: json['isActive'] as bool,
            visibilityStartsAt: json['visibilityStartsAt'] is String
                ? DateTime.parse(json['visibilityStartsAt'] as String)
                : null,
            visibilityEndsAt: json['visibilityEndsAt'] is String
                ? DateTime.parse(json['visibilityEndsAt'] as String)
                : null,
            items: const [],
          );
        })
        .toList(growable: false);
  }

  Future<List<MarketItem>> fetchItems() async {
    final raw = await _apiClient.getJsonList('/admin/market/items');
    return raw
        .cast<Map<String, dynamic>>()
        .map(MarketItem.fromJson)
        .toList(growable: false);
  }

  Future<List<MarketOrder>> fetchOrders(String playerId) async {
    final raw = await _apiClient.getJsonList(
      '/admin/market/orders/$playerId',
    );
    return raw
        .cast<Map<String, dynamic>>()
        .map(MarketOrder.fromJson)
        .toList(growable: false);
  }

  Future<void> createCategory({
    required String name,
    required int sortOrder,
    String? icon,
    DateTime? visibilityStartsAt,
    DateTime? visibilityEndsAt,
  }) async {
    await _apiClient.postJson(
      '/admin/market/categories',
      body: {
        'name': name,
        'sortOrder': sortOrder,
        'icon': icon,
        'visibilityStartsAt': visibilityStartsAt?.toIso8601String(),
        'visibilityEndsAt': visibilityEndsAt?.toIso8601String(),
      },
    );
  }

  Future<void> updateCategory({
    required String id,
    required String name,
    required int sortOrder,
    String? icon,
    DateTime? visibilityStartsAt,
    DateTime? visibilityEndsAt,
  }) async {
    await _apiClient.putJson(
      '/admin/market/categories/$id',
      body: {
        'name': name,
        'sortOrder': sortOrder,
        'icon': icon,
        'visibilityStartsAt': visibilityStartsAt?.toIso8601String(),
        'visibilityEndsAt': visibilityEndsAt?.toIso8601String(),
      },
    );
  }

  Future<void> createItem({
    required String categoryId,
    required MarketItemType itemType,
    required int quantity,
    required int price,
    int? promoPrice,
    DateTime? promotionStartsAt,
    DateTime? promotionEndsAt,
    int? maxStock,
    int? perPlayerLimit,
    PerPlayerLimitWindow perPlayerLimitWindow = PerPlayerLimitWindow.lifetime,
  }) async {
    await _apiClient.postJson(
      '/admin/market/items',
      body: _itemBody(
        categoryId: categoryId,
        itemType: itemType,
        quantity: quantity,
        price: price,
        promoPrice: promoPrice,
        promotionStartsAt: promotionStartsAt,
        promotionEndsAt: promotionEndsAt,
        maxStock: maxStock,
        perPlayerLimit: perPlayerLimit,
        perPlayerLimitWindow: perPlayerLimitWindow,
      ),
    );
  }

  Future<void> updateItem({
    required String id,
    required String categoryId,
    required MarketItemType itemType,
    required int quantity,
    required int price,
    required PerPlayerLimitWindow perPlayerLimitWindow,
    int? promoPrice,
    DateTime? promotionStartsAt,
    DateTime? promotionEndsAt,
    int? maxStock,
    int? perPlayerLimit,
  }) async {
    await _apiClient.putJson(
      '/admin/market/items/$id',
      body: _itemBody(
        categoryId: categoryId,
        itemType: itemType,
        quantity: quantity,
        price: price,
        promoPrice: promoPrice,
        promotionStartsAt: promotionStartsAt,
        promotionEndsAt: promotionEndsAt,
        maxStock: maxStock,
        perPlayerLimit: perPlayerLimit,
        perPlayerLimitWindow: perPlayerLimitWindow,
      ),
    );
  }

  Future<void> deactivateCategory(String id) async {
    await _apiClient.postJson('/admin/market/categories/$id/deactivate');
  }

  Future<void> deactivateItem(String id) async {
    await _apiClient.postJson('/admin/market/items/$id/deactivate');
  }

  Map<String, dynamic> _itemBody({
    required String categoryId,
    required MarketItemType itemType,
    required int quantity,
    required int price,
    required int? promoPrice,
    required DateTime? promotionStartsAt,
    required DateTime? promotionEndsAt,
    required int? maxStock,
    required int? perPlayerLimit,
    required PerPlayerLimitWindow perPlayerLimitWindow,
  }) {
    return {
      'categoryId': categoryId,
      'itemType': itemType.wire,
      'quantity': quantity,
      'price': price,
      'promoPrice': promoPrice,
      'promotionStartsAt': promotionStartsAt?.toIso8601String(),
      'promotionEndsAt': promotionEndsAt?.toIso8601String(),
      'maxStock': maxStock,
      'perPlayerLimit': perPlayerLimit,
      'perPlayerLimitWindow': perPlayerLimitWindow.wire,
    };
  }
}
