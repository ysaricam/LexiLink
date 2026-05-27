import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/market/data/market_models.dart';
import 'package:lexilink_app/features/market/data/market_repository.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum MarketStatus { initial, loading, loaded, buying, failure }

class MarketCubit extends Cubit<MarketState> {
  MarketCubit({required MarketRepository repository})
    : _repository = repository,
      super(const MarketState.initial());

  final MarketRepository _repository;

  Future<void> load() async {
    emit(state.copyWith(status: MarketStatus.loading, clearMessage: true));
    try {
      final categories = await _repository.fetchCategories();
      final orders = await _repository.fetchMyOrders();
      emit(
        state.copyWith(
          status: MarketStatus.loaded,
          categories: categories,
          orders: orders,
        ),
      );
    } on ApiException catch (e) {
      emit(state.copyWith(status: MarketStatus.failure, message: e.message));
    }
  }

  Future<void> buy(MarketItem item) async {
    emit(state.copyWith(status: MarketStatus.buying, clearMessage: true));
    try {
      await _repository.buy(item.id);
      final categories = await _repository.fetchCategories();
      final orders = await _repository.fetchMyOrders();
      emit(
        state.copyWith(
          status: MarketStatus.loaded,
          categories: categories,
          orders: orders,
          message: '${item.itemType.wire} +${item.quantity} purchased.',
        ),
      );
    } on ApiException catch (e) {
      emit(state.copyWith(status: MarketStatus.failure, message: e.message));
    }
  }
}

class MarketState extends Equatable {
  const MarketState({
    required this.status,
    required this.categories,
    required this.orders,
    this.message,
  });

  const MarketState.initial()
    : this(
        status: MarketStatus.initial,
        categories: const [],
        orders: const [],
      );

  final MarketStatus status;
  final List<MarketCategory> categories;
  final List<MarketOrder> orders;
  final String? message;

  MarketState copyWith({
    MarketStatus? status,
    List<MarketCategory>? categories,
    List<MarketOrder>? orders,
    String? message,
    bool clearMessage = false,
  }) {
    return MarketState(
      status: status ?? this.status,
      categories: categories ?? this.categories,
      orders: orders ?? this.orders,
      message: clearMessage ? null : (message ?? this.message),
    );
  }

  @override
  List<Object?> get props => [status, categories, orders, message];
}
