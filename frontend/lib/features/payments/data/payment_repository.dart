import 'package:lexilink_app/features/payments/data/payment_models.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class PaymentRepository {
  const PaymentRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<PaymentProduct>> fetchProducts(PaymentPlatform platform) async {
    final raw = await _apiClient.getJsonList(
      '/payments/products',
      queryParameters: {'platform': platform.wire},
    );
    return raw
        .cast<Map<String, dynamic>>()
        .map(PaymentProduct.fromJson)
        .toList(growable: false);
  }

  Future<PaymentVerifyResult> verifyPurchase({
    required PaymentPlatform platform,
    required StorePurchaseProof proof,
    required String clientRequestId,
  }) async {
    final body = <String, dynamic>{
      'platform': platform.wire,
      'storeProductId': proof.productId,
      'clientRequestId': clientRequestId,
      if (platform == PaymentPlatform.apple) ...{
        'storeTransactionId': proof.purchaseId,
        'signedTransactionJws': proof.verificationData,
        'accountToken': null,
      } else ...{
        'purchaseToken': proof.verificationData,
        'accountToken': null,
      },
    };

    final raw = await _apiClient.postJson('/payments/iap/verify', body: body);
    return PaymentVerifyResult.fromJson(raw);
  }
}
