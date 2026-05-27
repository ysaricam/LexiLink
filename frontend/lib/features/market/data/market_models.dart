import 'package:equatable/equatable.dart';

enum MarketItemType {
  energy('Energy', '⚡'),
  hint('Hint', '💡'),
  undo('Undo', '↶'),
  reset('Reset', '↻'),
  diamond('Diamond', '◆');

  const MarketItemType(this.wire, this.symbol);

  final String wire;
  final String symbol;

  static MarketItemType fromWire(String value) => MarketItemType.values
      .firstWhere((x) => x.wire.toLowerCase() == value.toLowerCase());
}

enum PerPlayerLimitWindow {
  lifetime('Lifetime'),
  daily('Daily'),
  perPromo('PerPromo');

  const PerPlayerLimitWindow(this.wire);

  final String wire;

  static PerPlayerLimitWindow fromWire(String value) =>
      PerPlayerLimitWindow.values.firstWhere(
        (x) => x.wire.toLowerCase() == value.toLowerCase(),
      );
}

DateTime? _date(dynamic value) =>
    value is String ? DateTime.parse(value) : null;

class MarketItem extends Equatable {
  const MarketItem({
    required this.id,
    required this.categoryId,
    required this.itemType,
    required this.quantity,
    required this.price,
    required this.effectivePrice,
    required this.soldCount,
    required this.perPlayerLimitWindow,
    required this.isActive,
    this.promoPrice,
    this.promotionStartsAt,
    this.promotionEndsAt,
    this.maxStock,
    this.remainingStock,
    this.perPlayerLimit,
    this.perPlayerRemaining,
  });

  factory MarketItem.fromJson(Map<String, dynamic> json) => MarketItem(
    id: json['id'] as String,
    categoryId: json['categoryId'] as String,
    itemType: MarketItemType.fromWire(json['itemType'] as String),
    quantity: json['quantity'] as int,
    price: json['price'] as int,
    effectivePrice: json['effectivePrice'] as int,
    promoPrice: json['promoPrice'] as int?,
    promotionStartsAt: _date(json['promotionStartsAt']),
    promotionEndsAt: _date(json['promotionEndsAt']),
    maxStock: json['maxStock'] as int?,
    soldCount: json['soldCount'] as int,
    remainingStock: json['remainingStock'] as int?,
    perPlayerLimit: json['perPlayerLimit'] as int?,
    perPlayerLimitWindow: PerPlayerLimitWindow.fromWire(
      json['perPlayerLimitWindow'] as String,
    ),
    perPlayerRemaining: json['perPlayerRemaining'] as int?,
    isActive: json['isActive'] as bool,
  );

  final String id;
  final String categoryId;
  final MarketItemType itemType;
  final int quantity;
  final int price;
  final int effectivePrice;
  final int? promoPrice;
  final DateTime? promotionStartsAt;
  final DateTime? promotionEndsAt;
  final int? maxStock;
  final int soldCount;
  final int? remainingStock;
  final int? perPlayerLimit;
  final PerPlayerLimitWindow perPlayerLimitWindow;
  final int? perPlayerRemaining;
  final bool isActive;

  bool get hasPromotion => promoPrice != null && effectivePrice < price;
  bool get isSoldOut => remainingStock != null && remainingStock! <= 0;
  bool get limitReached =>
      perPlayerRemaining != null && perPlayerRemaining! <= 0;

  @override
  List<Object?> get props => [
    id,
    categoryId,
    itemType,
    quantity,
    price,
    effectivePrice,
    promoPrice,
    promotionStartsAt,
    promotionEndsAt,
    maxStock,
    soldCount,
    remainingStock,
    perPlayerLimit,
    perPlayerLimitWindow,
    perPlayerRemaining,
    isActive,
  ];
}

class MarketCategory extends Equatable {
  const MarketCategory({
    required this.id,
    required this.name,
    required this.sortOrder,
    required this.isActive,
    required this.items,
    this.icon,
    this.visibilityStartsAt,
    this.visibilityEndsAt,
  });

  factory MarketCategory.fromJson(Map<String, dynamic> json) => MarketCategory(
    id: json['id'] as String,
    name: json['name'] as String,
    sortOrder: json['sortOrder'] as int,
    icon: json['icon'] as String?,
    isActive: json['isActive'] as bool,
    visibilityStartsAt: _date(json['visibilityStartsAt']),
    visibilityEndsAt: _date(json['visibilityEndsAt']),
    items: (json['items'] as List<dynamic>)
        .cast<Map<String, dynamic>>()
        .map(MarketItem.fromJson)
        .toList(growable: false),
  );

  final String id;
  final String name;
  final int sortOrder;
  final String? icon;
  final bool isActive;
  final DateTime? visibilityStartsAt;
  final DateTime? visibilityEndsAt;
  final List<MarketItem> items;

  @override
  List<Object?> get props => [
    id,
    name,
    sortOrder,
    icon,
    isActive,
    visibilityStartsAt,
    visibilityEndsAt,
    items,
  ];
}

class MarketOrder extends Equatable {
  const MarketOrder({
    required this.id,
    required this.playerId,
    required this.shopItemId,
    required this.itemType,
    required this.quantity,
    required this.diamondsPaid,
    required this.purchasedAt,
    required this.idempotencyKey,
  });

  factory MarketOrder.fromJson(Map<String, dynamic> json) => MarketOrder(
    id: json['id'] as String,
    playerId: json['playerId'] as String,
    shopItemId: json['shopItemId'] as String,
    itemType: MarketItemType.fromWire(json['itemType'] as String),
    quantity: json['quantity'] as int,
    diamondsPaid: json['diamondsPaid'] as int,
    purchasedAt: DateTime.parse(json['purchasedAt'] as String),
    idempotencyKey: json['idempotencyKey'] as String,
  );

  final String id;
  final String playerId;
  final String shopItemId;
  final MarketItemType itemType;
  final int quantity;
  final int diamondsPaid;
  final DateTime purchasedAt;
  final String idempotencyKey;

  @override
  List<Object?> get props => [
    id,
    playerId,
    shopItemId,
    itemType,
    quantity,
    diamondsPaid,
    purchasedAt,
    idempotencyKey,
  ];
}
