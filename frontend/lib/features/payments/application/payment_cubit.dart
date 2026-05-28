import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/payments/data/payment_models.dart';
import 'package:lexilink_app/features/payments/data/payment_repository.dart';
import 'package:lexilink_app/features/payments/data/payment_store_service.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum PaymentStatus {
  initial,
  loading,
  loaded,
  purchasing,
  verifying,
  success,
  unavailable,
  failure,
}

class PaymentCubit extends Cubit<PaymentState> {
  PaymentCubit({
    required PaymentRepository repository,
    required PaymentStoreService storeService,
    required PaymentPlatform? platform,
    bool isSupportedPlatform = true,
  }) : _repository = repository,
       _storeService = storeService,
       _isSupportedPlatform = isSupportedPlatform,
       super(PaymentState.initial(platform: platform)) {
    _purchaseSubscription = _storeService.purchases.listen(
      _handlePurchaseProof,
    );
  }

  final PaymentRepository _repository;
  final PaymentStoreService _storeService;
  final bool _isSupportedPlatform;
  StreamSubscription<StorePurchaseProof>? _purchaseSubscription;

  Future<void> load() async {
    final platform = state.platform;
    if (!_isSupportedPlatform || platform == null) {
      emit(
        state.copyWith(
          status: PaymentStatus.unavailable,
          message: 'Diamonds purchase is available on iOS and Android.',
        ),
      );
      return;
    }

    emit(state.copyWith(status: PaymentStatus.loading, clearMessage: true));
    try {
      final storeAvailable = await _storeService.isAvailable();
      if (!storeAvailable) {
        emit(
          state.copyWith(
            status: PaymentStatus.unavailable,
            message: 'The store is unavailable on this device.',
          ),
        );
        return;
      }

      final products = await _repository.fetchProducts(platform);
      final activeProductIds = products
          .where((product) => product.isActive)
          .map((product) => product.storeProductId)
          .toSet();
      final storeProducts = activeProductIds.isEmpty
          ? const <StorePaymentProduct>[]
          : await _storeService.loadProducts(activeProductIds);
      final storeById = {
        for (final product in storeProducts) product.id: product,
      };
      final bundles = products
          .map(
            (product) => DiamondBundle(
              product: product,
              storeProduct: storeById[product.storeProductId],
            ),
          )
          .toList(growable: false);

      emit(state.copyWith(status: PaymentStatus.loaded, bundles: bundles));
    } on ApiException catch (e) {
      emit(state.copyWith(status: PaymentStatus.failure, message: e.message));
    } on Object catch (_) {
      emit(
        state.copyWith(
          status: PaymentStatus.failure,
          message: 'Could not load diamond bundles.',
        ),
      );
    }
  }

  Future<void> buy(DiamondBundle bundle) async {
    final storeProduct = bundle.storeProduct;
    if (!bundle.isAvailable || storeProduct == null) {
      emit(
        state.copyWith(
          status: PaymentStatus.failure,
          message: 'This diamond bundle is unavailable.',
        ),
      );
      return;
    }

    emit(
      state.copyWith(
        status: PaymentStatus.purchasing,
        selectedProductId: bundle.product.storeProductId,
        clearMessage: true,
      ),
    );
    try {
      await _storeService.buy(storeProduct);
    } on Object catch (_) {
      emit(
        state.copyWith(
          status: PaymentStatus.failure,
          clearSelectedProductId: true,
          message: 'Purchase could not be started.',
        ),
      );
    }
  }

  Future<void> _handlePurchaseProof(StorePurchaseProof proof) async {
    final platform = state.platform;
    if (platform == null) return;

    emit(
      state.copyWith(
        status: PaymentStatus.verifying,
        selectedProductId: proof.productId,
        clearMessage: true,
      ),
    );

    try {
      final result = await _repository.verifyPurchase(
        platform: platform,
        proof: proof,
        clientRequestId: _clientRequestId(proof),
      );
      if (result.canFinishTransaction) {
        await _storeService.finish(proof);
      }

      if (!result.isGranted) {
        emit(
          state.copyWith(
            status: PaymentStatus.failure,
            clearSelectedProductId: true,
            message:
                result.postProcessingFailureReason ??
                'Purchase delivery is pending.',
          ),
        );
        return;
      }

      emit(
        state.copyWith(
          status: PaymentStatus.success,
          clearSelectedProductId: true,
          message: '+${result.diamondAmount} diamonds added.',
        ),
      );
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: PaymentStatus.failure,
          clearSelectedProductId: true,
          message: e.message,
        ),
      );
    } on Object catch (_) {
      emit(
        state.copyWith(
          status: PaymentStatus.failure,
          clearSelectedProductId: true,
          message: 'Purchase verification failed.',
        ),
      );
    }
  }

  String _clientRequestId(StorePurchaseProof proof) {
    final id = proof.purchaseId;
    if (id != null && id.isNotEmpty) {
      return 'iap-${proof.productId}-$id';
    }
    return 'iap-${proof.productId}-${DateTime.now().microsecondsSinceEpoch}';
  }

  @override
  Future<void> close() async {
    await _purchaseSubscription?.cancel();
    return super.close();
  }
}

class PaymentState extends Equatable {
  const PaymentState({
    required this.status,
    required this.platform,
    required this.bundles,
    this.selectedProductId,
    this.message,
  });

  const PaymentState.initial({required PaymentPlatform? platform})
    : this(
        status: PaymentStatus.initial,
        platform: platform,
        bundles: const [],
      );

  final PaymentStatus status;
  final PaymentPlatform? platform;
  final List<DiamondBundle> bundles;
  final String? selectedProductId;
  final String? message;

  bool get isBusy =>
      status == PaymentStatus.loading ||
      status == PaymentStatus.purchasing ||
      status == PaymentStatus.verifying;

  PaymentState copyWith({
    PaymentStatus? status,
    PaymentPlatform? platform,
    List<DiamondBundle>? bundles,
    String? selectedProductId,
    String? message,
    bool clearMessage = false,
    bool clearSelectedProductId = false,
  }) {
    return PaymentState(
      status: status ?? this.status,
      platform: platform ?? this.platform,
      bundles: bundles ?? this.bundles,
      selectedProductId: clearSelectedProductId
          ? null
          : (selectedProductId ?? this.selectedProductId),
      message: clearMessage ? null : (message ?? this.message),
    );
  }

  @override
  List<Object?> get props => [
    status,
    platform,
    bundles,
    selectedProductId,
    message,
  ];
}
