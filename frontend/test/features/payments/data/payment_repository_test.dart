import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/payments/data/payment_models.dart';
import 'package:lexilink_app/features/payments/data/payment_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

const _productsBody =
    '[{"id":"p1","storeProductId":"diamond_100",'
    '"diamondAmount":100,"isActive":true},'
    '{"id":"p2","storeProductId":"diamond_550",'
    '"diamondAmount":550,"isActive":true},'
    '{"id":"p3","storeProductId":"diamond_1200",'
    '"diamondAmount":1200,"isActive":true},'
    '{"id":"p4","storeProductId":"diamond_2500",'
    '"diamondAmount":2500,"isActive":true}]';
const _appleVerifyBody =
    '{"paymentId":"pay-1","productId":"p1","diamondAmount":100,'
    '"status":"Granted","canFinishTransaction":true,"isReplay":false}';
const _googleVerifyBody =
    '{"paymentId":"pay-2","productId":"p2","diamondAmount":250,'
    '"status":"Granted","canFinishTransaction":false,"isReplay":true}';

PaymentRepository _repository(MockClientHandler handler) {
  return PaymentRepository(
    apiClient: ApiClient(
      config: const ApiConfig(baseUrl: 'http://localhost:5000'),
      tokenStore: InMemoryTokenStore(),
      httpClient: MockClient(handler),
    ),
  );
}

void main() {
  group('PaymentRepository', () {
    test('fetchProducts calls platform-filtered catalog endpoint', () async {
      final repository = _repository((request) async {
        expect(request.method, 'GET');
        expect(request.url.path, '/payments/products');
        expect(request.url.queryParameters['platform'], 'Apple');
        return http.Response(_productsBody, 200);
      });

      final products = await repository.fetchProducts(PaymentPlatform.apple);

      expect(
        products.map((product) => product.storeProductId),
        ['diamond_100', 'diamond_550', 'diamond_1200', 'diamond_2500'],
      );
      expect(
        products.map((product) => product.diamondAmount),
        [100, 550, 1200, 2500],
      );
    });

    test('verifyPurchase sends Apple transaction proof', () async {
      final repository = _repository((request) async {
        expect(request.method, 'POST');
        expect(request.url.path, '/payments/iap/verify');
        final body = jsonDecode(request.body) as Map<String, dynamic>;
        expect(body['platform'], 'Apple');
        expect(body['storeProductId'], 'diamond_100');
        expect(body['storeTransactionId'], 'tx-1');
        expect(body['signedTransactionJws'], 'signed-jws');
        expect(body['clientRequestId'], 'request-1');
        return http.Response(_appleVerifyBody, 200);
      });

      final result = await repository.verifyPurchase(
        platform: PaymentPlatform.apple,
        proof: const StorePurchaseProof(
          productId: 'diamond_100',
          purchaseId: 'tx-1',
          verificationData: 'signed-jws',
        ),
        clientRequestId: 'request-1',
      );

      expect(result.isGranted, isTrue);
      expect(result.canFinishTransaction, isTrue);
    });

    test('verifyPurchase sends Google purchase token', () async {
      final repository = _repository((request) async {
        final body = jsonDecode(request.body) as Map<String, dynamic>;
        expect(body['platform'], 'Google');
        expect(body['storeProductId'], 'diamond_2500');
        expect(body['purchaseToken'], 'token-1');
        expect(body.containsKey('storeTransactionId'), isFalse);
        return http.Response(_googleVerifyBody, 200);
      });

      final result = await repository.verifyPurchase(
        platform: PaymentPlatform.google,
        proof: const StorePurchaseProof(
          productId: 'diamond_2500',
          verificationData: 'token-1',
        ),
        clientRequestId: 'request-2',
      );

      expect(result.isReplay, isTrue);
      expect(result.canFinishTransaction, isFalse);
    });
  });
}
