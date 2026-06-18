import 'dart:async';

import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/payments/application/payment_cubit.dart';
import 'package:lexilink_app/features/payments/data/payment_models.dart';
import 'package:lexilink_app/features/payments/data/payment_repository.dart';
import 'package:lexilink_app/features/payments/data/payment_store_service.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

const _productsBody =
    '[{"id":"p1","storeProductId":"diamonds_100",'
    '"diamondAmount":100,"isActive":true}]';
const _grantedBody =
    '{"paymentId":"pay-1","productId":"p1","diamondAmount":100,'
    '"status":"Granted","canFinishTransaction":true,"isReplay":false}';

void main() {
  group('PaymentCubit', () {
    blocTest<PaymentCubit, PaymentState>(
      'marks unsupported platforms unavailable',
      build: () => _buildCubit(
        isSupportedPlatform: false,
        platform: null,
        handler: (_) async => http.Response('[]', 200),
      ),
      act: (cubit) => cubit.load(),
      verify: (cubit) {
        expect(cubit.state.status, PaymentStatus.unavailable);
        expect(cubit.state.message, contains('iOS and Android'));
      },
    );

    blocTest<PaymentCubit, PaymentState>(
      'loads backend products and merges store prices',
      build: () => _buildCubit(
        storeService: _FakePaymentStoreService(
          products: const [
            StorePaymentProduct(
              id: 'diamonds_100',
              title: '100 Diamonds',
              description: 'Small pack',
              price: r'$0.99',
            ),
          ],
        ),
        handler: (request) async {
          expect(request.url.path, '/payments/products');
          expect(request.url.queryParameters['platform'], 'Apple');
          return http.Response(_productsBody, 200);
        },
      ),
      act: (cubit) => cubit.load(),
      verify: (cubit) {
        expect(cubit.state.status, PaymentStatus.loaded);
        expect(cubit.state.bundles.single.displayPrice, r'$0.99');
        expect(cubit.state.bundles.single.isAvailable, isTrue);
      },
    );

    test('does not emit after close when load completes later', () async {
      final availability = Completer<bool>();
      var requestedProducts = false;
      final cubit = _buildCubit(
        storeService: _FakePaymentStoreService(
          availability: availability.future,
        ),
        handler: (_) async {
          requestedProducts = true;
          return http.Response(_productsBody, 200);
        },
      );

      final load = cubit.load();
      await Future<void>.delayed(Duration.zero);
      await cubit.close();

      availability.complete(true);
      await load;

      expect(requestedProducts, isFalse);
      expect(cubit.isClosed, isTrue);
    });

    blocTest<PaymentCubit, PaymentState>(
      'verifies purchase proof and finishes when backend allows it',
      build: () {
        final store = _FakePaymentStoreService(
          products: const [
            StorePaymentProduct(
              id: 'diamonds_100',
              title: '100 Diamonds',
              description: 'Small pack',
              price: r'$0.99',
            ),
          ],
        );
        return _buildCubit(
          storeService: store,
          handler: (request) async {
            if (request.url.path == '/payments/products') {
              return http.Response(_productsBody, 200);
            }

            expect(request.url.path, '/payments/iap/verify');
            return http.Response(_grantedBody, 200);
          },
        )..load();
      },
      act: (cubit) async {
        await Future<void>.delayed(Duration.zero);
        cubit.debugStore.addPurchase(
          const StorePurchaseProof(
            productId: 'diamonds_100',
            purchaseId: 'tx-1',
            verificationData: 'signed-jws',
          ),
        );
      },
      wait: const Duration(milliseconds: 10),
      verify: (cubit) {
        final store = cubit.debugStore;
        expect(cubit.state.status, PaymentStatus.success);
        expect(cubit.state.message, '+100 diamonds added.');
        expect(store.finishedProofs, hasLength(1));
        expect(store.finishedProofs.single.purchaseId, 'tx-1');
      },
    );
  });
}

PaymentCubit _buildCubit({
  required MockClientHandler handler,
  PaymentPlatform? platform = PaymentPlatform.apple,
  bool isSupportedPlatform = true,
  _FakePaymentStoreService? storeService,
}) {
  final store = storeService ?? _FakePaymentStoreService();
  return _DebugPaymentCubit(
    repository: PaymentRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient(handler),
      ),
    ),
    storeService: store,
    platform: platform,
    isSupportedPlatform: isSupportedPlatform,
    debugStore: store,
  );
}

class _DebugPaymentCubit extends PaymentCubit {
  _DebugPaymentCubit({
    required super.repository,
    required super.storeService,
    required super.platform,
    required super.isSupportedPlatform,
    required _FakePaymentStoreService debugStore,
  }) : _debugStore = debugStore;

  final _FakePaymentStoreService _debugStore;
}

extension on PaymentCubit {
  _FakePaymentStoreService get debugStore =>
      (this as _DebugPaymentCubit)._debugStore;
}

class _FakePaymentStoreService implements PaymentStoreService {
  _FakePaymentStoreService({
    this.products = const [],
    Future<bool>? availability,
  }) : _availability = availability;

  final List<StorePaymentProduct> products;
  final Future<bool>? _availability;
  final List<StorePurchaseProof> finishedProofs = [];
  final _controller = StreamController<StorePurchaseProof>.broadcast();

  @override
  Stream<StorePurchaseProof> get purchases => _controller.stream;

  @override
  Future<bool> isAvailable() async => _availability ?? true;

  @override
  Future<List<StorePaymentProduct>> loadProducts(Set<String> productIds) async {
    return products
        .where((product) => productIds.contains(product.id))
        .toList(growable: false);
  }

  @override
  Future<void> buy(StorePaymentProduct product) async {}

  @override
  Future<void> finish(StorePurchaseProof proof) async {
    finishedProofs.add(proof);
  }

  void addPurchase(StorePurchaseProof proof) {
    _controller.add(proof);
  }
}
