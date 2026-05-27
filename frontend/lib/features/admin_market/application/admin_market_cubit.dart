import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/admin_market/data/admin_market_repository.dart';
import 'package:lexilink_app/features/market/data/market_models.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum AdminMarketStatus { initial, loading, loaded, saving, failure }

class AdminMarketCubit extends Cubit<AdminMarketState> {
  AdminMarketCubit({required AdminMarketRepository repository})
    : _repository = repository,
      super(const AdminMarketState.initial());

  final AdminMarketRepository _repository;

  Future<void> load() async {
    emit(state.copyWith(status: AdminMarketStatus.loading, clearError: true));
    try {
      final categories = await _repository.fetchCategories();
      final items = await _repository.fetchItems();
      emit(
        state.copyWith(
          status: AdminMarketStatus.loaded,
          categories: categories,
          items: items,
        ),
      );
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminMarketStatus.failure,
          errorMessage: e.message,
        ),
      );
    }
  }

  Future<void> saveCategory({
    required String name,
    required int sortOrder,
    String? icon,
    DateTime? visibilityStartsAt,
    DateTime? visibilityEndsAt,
    String? id,
  }) async {
    emit(state.copyWith(status: AdminMarketStatus.saving, clearError: true));
    try {
      if (id == null) {
        await _repository.createCategory(
          name: name,
          sortOrder: sortOrder,
          icon: icon,
          visibilityStartsAt: visibilityStartsAt,
          visibilityEndsAt: visibilityEndsAt,
        );
      } else {
        await _repository.updateCategory(
          id: id,
          name: name,
          sortOrder: sortOrder,
          icon: icon,
          visibilityStartsAt: visibilityStartsAt,
          visibilityEndsAt: visibilityEndsAt,
        );
      }
      await load();
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminMarketStatus.failure,
          errorMessage: e.message,
        ),
      );
    }
  }

  Future<void> saveItem({
    required String categoryId,
    required MarketItemType itemType,
    required int quantity,
    required int price,
    required PerPlayerLimitWindow perPlayerLimitWindow,
    String? id,
    int? promoPrice,
    DateTime? promotionStartsAt,
    DateTime? promotionEndsAt,
    int? maxStock,
    int? perPlayerLimit,
  }) async {
    emit(state.copyWith(status: AdminMarketStatus.saving, clearError: true));
    try {
      if (id == null) {
        await _repository.createItem(
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
        );
      } else {
        await _repository.updateItem(
          id: id,
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
        );
      }
      await load();
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminMarketStatus.failure,
          errorMessage: e.message,
        ),
      );
    }
  }

  Future<void> loadOrders(String playerId) async {
    emit(state.copyWith(status: AdminMarketStatus.loading, clearError: true));
    try {
      final orders = await _repository.fetchOrders(playerId);
      emit(
        state.copyWith(
          status: AdminMarketStatus.loaded,
          orders: orders,
          orderPlayerId: playerId,
        ),
      );
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminMarketStatus.failure,
          errorMessage: e.message,
        ),
      );
    }
  }

  Future<void> deactivateCategory(String id) async {
    emit(state.copyWith(status: AdminMarketStatus.saving, clearError: true));
    try {
      await _repository.deactivateCategory(id);
      await load();
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminMarketStatus.failure,
          errorMessage: e.message,
        ),
      );
    }
  }

  Future<void> deactivateItem(String id) async {
    emit(state.copyWith(status: AdminMarketStatus.saving, clearError: true));
    try {
      await _repository.deactivateItem(id);
      await load();
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminMarketStatus.failure,
          errorMessage: e.message,
        ),
      );
    }
  }
}

class AdminMarketState extends Equatable {
  const AdminMarketState({
    required this.status,
    required this.categories,
    required this.items,
    this.orders = const [],
    this.orderPlayerId,
    this.errorMessage,
  });

  const AdminMarketState.initial()
    : this(
        status: AdminMarketStatus.initial,
        categories: const [],
        items: const [],
      );

  final AdminMarketStatus status;
  final List<MarketCategory> categories;
  final List<MarketItem> items;
  final List<MarketOrder> orders;
  final String? orderPlayerId;
  final String? errorMessage;

  AdminMarketState copyWith({
    AdminMarketStatus? status,
    List<MarketCategory>? categories,
    List<MarketItem>? items,
    List<MarketOrder>? orders,
    String? orderPlayerId,
    String? errorMessage,
    bool clearError = false,
  }) {
    return AdminMarketState(
      status: status ?? this.status,
      categories: categories ?? this.categories,
      items: items ?? this.items,
      orders: orders ?? this.orders,
      orderPlayerId: orderPlayerId ?? this.orderPlayerId,
      errorMessage: clearError ? null : (errorMessage ?? this.errorMessage),
    );
  }

  @override
  List<Object?> get props => [
    status,
    categories,
    items,
    orders,
    orderPlayerId,
    errorMessage,
  ];
}
