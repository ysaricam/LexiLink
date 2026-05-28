import 'dart:async';

import 'package:in_app_purchase/in_app_purchase.dart';
import 'package:lexilink_app/features/payments/data/payment_models.dart';

abstract class PaymentStoreService {
  Stream<StorePurchaseProof> get purchases;

  Future<bool> isAvailable();

  Future<List<StorePaymentProduct>> loadProducts(Set<String> productIds);

  Future<void> buy(StorePaymentProduct product);

  Future<void> finish(StorePurchaseProof proof);
}

class InAppPurchaseStoreService implements PaymentStoreService {
  InAppPurchaseStoreService({InAppPurchase? inAppPurchase})
    : _iap = inAppPurchase ?? InAppPurchase.instance;

  final InAppPurchase _iap;
  final Map<String, PurchaseDetails> _pendingPurchases = {};

  @override
  Stream<StorePurchaseProof> get purchases =>
      _iap.purchaseStream.expand(_proofsFromPurchaseDetails);

  @override
  Future<bool> isAvailable() => _iap.isAvailable();

  @override
  Future<List<StorePaymentProduct>> loadProducts(Set<String> productIds) async {
    final response = await _iap.queryProductDetails(productIds);
    return response.productDetails
        .map(
          (product) => StorePaymentProduct(
            id: product.id,
            title: product.title,
            description: product.description,
            price: product.price,
          ),
        )
        .toList(growable: false);
  }

  @override
  Future<void> buy(StorePaymentProduct product) async {
    final response = await _iap.queryProductDetails({product.id});
    final productDetails = response.productDetails.firstWhere(
      (x) => x.id == product.id,
    );
    final param = PurchaseParam(productDetails: productDetails);
    await _iap.buyConsumable(purchaseParam: param, autoConsume: false);
  }

  @override
  Future<void> finish(StorePurchaseProof proof) async {
    final purchase = _pendingPurchases.remove(_proofKey(proof));
    if (purchase != null && purchase.pendingCompletePurchase) {
      await _iap.completePurchase(purchase);
    }
  }

  Iterable<StorePurchaseProof> _proofsFromPurchaseDetails(
    List<PurchaseDetails> details,
  ) sync* {
    for (final purchase in details) {
      if (purchase.status == PurchaseStatus.purchased ||
          purchase.status == PurchaseStatus.restored) {
        final proof = StorePurchaseProof(
          productId: purchase.productID,
          purchaseId: purchase.purchaseID,
          verificationData: purchase.verificationData.serverVerificationData,
        );
        if (purchase.pendingCompletePurchase) {
          _pendingPurchases[_proofKey(proof)] = purchase;
        }
        yield proof;
      }
    }
  }

  String _proofKey(StorePurchaseProof proof) {
    final purchaseId = proof.purchaseId;
    if (purchaseId != null && purchaseId.isNotEmpty) {
      return '${proof.productId}:$purchaseId';
    }
    return proof.productId;
  }
}
